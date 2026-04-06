namespace Aiursoft.Template.Services.BackgroundJobs;

/// <summary>
/// Defines the scheduling configuration for a background job.
/// Registered with <c>services.AddScheduledTask&lt;TJob&gt;()</c>.
/// The corresponding job must first be registered with
/// <c>services.RegisterBackgroundJob&lt;TJob&gt;()</c>.
/// </summary>
public class ScheduledTaskRegistration
{
    /// <summary>
    /// The job type to run on schedule.
    /// Must be a class registered via <c>RegisterBackgroundJob&lt;TJob&gt;()</c>.
    /// </summary>
    public required Type JobType { get; init; }

    /// <summary>How long to wait after application start before the first automatic run.</summary>
    public required TimeSpan StartDelay { get; init; }

    /// <summary>How often the job is triggered automatically after the first run.</summary>
    public required TimeSpan Period { get; init; }
}
