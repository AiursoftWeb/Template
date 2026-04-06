namespace Aiursoft.Template.Services.BackgroundJobs;

/// <summary>
/// Extension methods for registering background jobs into the registry.
/// </summary>
public static class BackgroundJobRegistryExtensions
{
    /// <summary>
    /// Registers a background job that will appear in the admin UI at <c>/Jobs</c>
    /// and returns its registration object.
    /// It can be triggered manually from there, or programmatically via
    /// <c>BackgroundJobRegistry.TriggerNow&lt;TJob&gt;()</c> (fire and forget).
    /// <para>
    /// To also run it on a schedule, pass the returned registration to
    /// <c>services.RegisterScheduledTask(registration, period, startDelay)</c>.
    /// </para>
    /// </summary>
    /// <typeparam name="TJob">A class that implements <see cref="IBackgroundJob"/>.</typeparam>
    /// <param name="services">The service collection.</param>
    public static RegisteredJob RegisterBackgroundJob<TJob>(
        this IServiceCollection services)
        where TJob : class, IBackgroundJob
    {
        // Register the implementation so it can be resolved from a DI scope at execution time.
        services.AddTransient<TJob>();

        // Register its identity metadata as a singleton (picked up by BackgroundJobRegistry).
        var registration = new RegisteredJob
        {
            JobType = typeof(TJob)
        };
        services.AddSingleton(registration);

        return registration;
    }

    /// <summary>
    /// Backward-compatible alias of <see cref="RegisterBackgroundJob{TJob}(IServiceCollection)"/>.
    /// </summary>
    [Obsolete("Use RegisterBackgroundJob<TJob>() instead.")]
    public static IServiceCollection AddBackgroundJob<TJob>(this IServiceCollection services)
        where TJob : class, IBackgroundJob
    {
        _ = services.RegisterBackgroundJob<TJob>();
        return services;
    }
}

