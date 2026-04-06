namespace Aiursoft.Template.Services.BackgroundJobs;

/// <summary>
/// A hosted service that reads all <see cref="ScheduledTaskRegistration"/> instances
/// from DI and creates a per-job timer that automatically enqueues the job after its
/// <see cref="ScheduledTaskRegistration.StartDelay"/> and then every
/// <see cref="ScheduledTaskRegistration.Period"/>.
/// </summary>
public class JobSchedulerService(
    BackgroundJobRegistry registry,
    IEnumerable<ScheduledTaskRegistration> scheduledTasks,
    ILogger<JobSchedulerService> logger) : IHostedService, IDisposable
{
    private readonly List<Timer> _timers = [];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var tasks = scheduledTasks.ToList();

        if (tasks.Count == 0)
        {
            logger.LogInformation("Job Scheduler: no scheduled tasks registered.");
            return Task.CompletedTask;
        }

        foreach (var task in tasks)
        {
            var captured = task; // capture loop variable
            logger.LogInformation(
                "Job Scheduler: scheduling {JobType} every {Period} (first run after {StartDelay})",
                captured.JobType.Name, captured.Period, captured.StartDelay);

            var timer = new Timer(
                callback: _ => EnqueueJob(captured),
                state: null,
                dueTime: captured.StartDelay,
                period:  captured.Period);

            _timers.Add(timer);
        }

        return Task.CompletedTask;
    }

    private void EnqueueJob(ScheduledTaskRegistration task)
    {
        try
        {
            var jobId = registry.TriggerNow(task.JobType);
            logger.LogInformation(
                "Job Scheduler: enqueued scheduled run of {JobType} (jobId={JobId})",
                task.JobType.Name, jobId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Job Scheduler: failed to enqueue scheduled run of {JobType}",
                task.JobType.Name);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Job Scheduler: stopping all timers.");
        foreach (var timer in _timers)
        {
            timer.Change(Timeout.Infinite, 0);
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        foreach (var timer in _timers)
        {
            timer.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
