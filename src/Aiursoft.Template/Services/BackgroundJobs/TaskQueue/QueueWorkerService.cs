using Aiursoft.Template.Models.BackgroundJobs;

namespace Aiursoft.Template.Services.BackgroundJobs;

/// <summary>
/// Background service that processes jobs from the CanonQueue.
/// </summary>
public class QueueWorkerService(
    ServiceTaskQueue taskQueue,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<QueueWorkerService> logger) : IHostedService, IDisposable
{
    private Timer? _timer;
    private Timer? _cleanupTimer;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Task Queue Worker is starting");

        // Process jobs every 100ms
        _timer = new Timer(ProcessJobs, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));

        // Cleanup old jobs every 5 minutes
        _cleanupTimer = new Timer(CleanupOldJobs, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5));

        return Task.CompletedTask;
    }

    private void ProcessJobs(object? state)
    {
        // Try to acquire the semaphore (non-blocking)
        if (!_semaphore.Wait(0))
        {
            return; // Already processing
        }

        try
        {
            var queues = taskQueue.GetQueuesWithPendingJobs().ToList();

            foreach (var queueName in queues)
            {
                // Try to get next job for this queue (will return null if queue is already processing)
                var job = taskQueue.TryDequeueNextJob(queueName);
                if (job != null)
                {
                    // Process job asynchronously without blocking the timer
                    _ = Task.Run(async () => await ProcessJobAsync(job));
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task ProcessJobAsync(JobInfo job)
    {
        try
        {
            logger.LogInformation("Processing task {TaskId} ({TaskName}) from queue {QueueName}",
                job.JobId, job.JobName, job.QueueName);

            // Create a scope for dependency injection
            using var scope = serviceScopeFactory.CreateScope();

            // Resolve the service
            var service = scope.ServiceProvider.GetRequiredService(job.ServiceType);

            // Execute the job
            await job.JobAction(service);

            // Mark as success
            taskQueue.CompleteJob(job.JobId, true);

            logger.LogInformation("Task {TaskId} ({TaskName}) completed successfully",
                job.JobId, job.JobName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Task {TaskId} ({TaskName}) failed with error: {Error}",
                job.JobId, job.JobName, ex.Message);

            // Mark as failed with error message
            taskQueue.CompleteJob(job.JobId, false, ex.ToString());
        }
    }

    private void CleanupOldJobs(object? state)
    {
        try
        {
            logger.LogInformation("Task Queue Worker: cleaning up old task records");
            taskQueue.CleanupOldJobs(TimeSpan.FromHours(1));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Task Queue Worker: failed to clean up old task records");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Task Queue Worker is stopping");

        _timer?.Change(Timeout.Infinite, 0);
        _cleanupTimer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _cleanupTimer?.Dispose();
        _semaphore.Dispose();
        GC.SuppressFinalize(this);
    }
}
