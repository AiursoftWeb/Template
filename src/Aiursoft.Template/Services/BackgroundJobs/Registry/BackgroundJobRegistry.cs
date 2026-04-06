namespace Aiursoft.Template.Services.BackgroundJobs;

/// <summary>
/// A singleton registry of all background jobs registered with
/// <c>services.RegisterBackgroundJob&lt;TJob&gt;()</c>.
/// <para>
/// Provides metadata for the admin UI (at <c>/Jobs</c>) and a
/// <see cref="TriggerNow{TJob}"/> method for fire-and-forget triggering
/// from any Controller or service.
/// </para>
/// </summary>
public class BackgroundJobRegistry(
    ServiceTaskQueue queue,
    IServiceScopeFactory serviceScopeFactory,
    IEnumerable<RegisteredJob> registrations)
{
    private readonly IReadOnlyList<RegisteredJob> _registrations =
        registrations.ToList().AsReadOnly();

    // ── Query ─────────────────────────────────────────────────────────────────

    /// <summary>Returns all registered job definitions (for the admin UI).</summary>
    public IReadOnlyList<RegisteredJob> GetAll() =>
        _registrations
            .Select(r => BuildSummary(r.JobType))
            .ToList()
            .AsReadOnly();

    /// <summary>Finds a registered job by job type.</summary>
    public RegisteredJob? FindByType(Type jobType) =>
        _registrations.FirstOrDefault(r => r.JobType == jobType);

    /// <summary>Finds a registered job by the job type's short name.</summary>
    public RegisteredJob? FindByTypeName(string typeName) =>
        _registrations.FirstOrDefault(r => r.JobType.Name == typeName);

    // ── Fire-and-Forget Trigger ───────────────────────────────────────────────

    /// <summary>
    /// Enqueues an immediate, one-off execution of <typeparamref name="TJob"/>.
    /// Safe to call from any Controller or service — fire and forget.
    /// </summary>
    public Guid TriggerNow<TJob>() where TJob : class, IBackgroundJob =>
        TriggerNow(typeof(TJob));

    /// <summary>
    /// Enqueues an immediate, one-off execution of the job identified by its type.
    /// Used by <c>JobsController</c> when an admin clicks "Trigger Now".
    /// </summary>
    public Guid TriggerNow(Type jobType)
    {
        var reg = FindByType(jobType)
            ?? throw new InvalidOperationException(
                $"Job type '{jobType.Name}' is not registered. " +
                "Make sure you called services.RegisterBackgroundJob<TJob>().");

        var summary = BuildSummary(reg.JobType);

        return queue.QueueWithDependency<IBackgroundJob>(
            queueName:  jobType.Name,
            jobName:    $"{summary.Name ?? jobType.Name} (manual trigger {DateTime.UtcNow:HH:mm:ss})",
            job:        j => j.ExecuteAsync(),
            serviceType: jobType);
    }

    /// <summary>
    /// Enqueues an immediate, one-off execution by job type name string.
    /// Convenience overload for the admin UI (form POST carries a string).
    /// </summary>
    public Guid TriggerNow(string jobTypeName)
    {
        var reg = FindByTypeName(jobTypeName)
            ?? throw new InvalidOperationException(
                $"No registered job with type name '{jobTypeName}'.");
        return TriggerNow(reg.JobType);
    }

    private RegisteredJob BuildSummary(Type jobType)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService(jobType) as IBackgroundJob
            ?? throw new InvalidOperationException(
                $"Registered job type '{jobType.Name}' does not implement IBackgroundJob.");

        return new RegisteredJob
        {
            JobType = jobType,
            Name = job.Name,
            Description = job.Description
        };
    }
}

