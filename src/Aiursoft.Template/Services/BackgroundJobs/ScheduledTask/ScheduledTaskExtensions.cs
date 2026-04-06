namespace Aiursoft.Template.Services.BackgroundJobs;

/// <summary>
/// Extension methods for registering scheduled tasks.
/// </summary>
public static class ScheduledTaskExtensions
{
    /// <summary>
    /// Attaches a recurring schedule to an existing background job registration.
    /// The job will run automatically after <paramref name="startDelay"/>,
    /// then repeat every <paramref name="period"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="registration">A registration returned by <c>RegisterBackgroundJob</c>.</param>
    /// <param name="period">How often the job is triggered. Defaults to 3 hours.</param>
    /// <param name="startDelay">Delay before first run after app start. Defaults to 3 minutes.</param>
    public static IServiceCollection RegisterScheduledTask(
        this IServiceCollection services,
        RegisteredJob registration,
        TimeSpan? period = null,
        TimeSpan? startDelay = null)
    {
        ArgumentNullException.ThrowIfNull(registration);

        services.AddSingleton(new ScheduledTaskRegistration
        {
            JobType = registration.JobType,
            Period = period ?? TimeSpan.FromHours(3),
            StartDelay = startDelay ?? TimeSpan.FromMinutes(3)
        });

        return services;
    }

    /// <summary>
    /// Attaches a recurring schedule to a background job that was already registered
    /// with <c>services.RegisterBackgroundJob&lt;TJob&gt;()</c>.
    /// The job will run automatically after <paramref name="startDelay"/>,
    /// then repeat every <paramref name="period"/>.
    /// </summary>
    /// <typeparam name="TJob">The job type, must also be registered via RegisterBackgroundJob.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="period">How often the job is triggered. Defaults to 3 hours.</param>
    /// <param name="startDelay">Delay before first run after app start. Defaults to 3 minutes.</param>
    [Obsolete("Use RegisterScheduledTask(registration, period, startDelay) instead.")]
    public static IServiceCollection AddScheduledTask<TJob>(
        this IServiceCollection services,
        TimeSpan? period = null,
        TimeSpan? startDelay = null)
        where TJob : class, IBackgroundJob
    {
        services.RegisterScheduledTask(
            registration: new RegisteredJob { JobType = typeof(TJob) },
            period: period,
            startDelay: startDelay);

        return services;
    }
}
