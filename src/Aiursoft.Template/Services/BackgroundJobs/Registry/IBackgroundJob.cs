namespace Aiursoft.Template.Services.BackgroundJobs;

/// <summary>
/// Represents a background job that can be scheduled and executed by the background job system.
/// Implement this interface and register with <c>services.RegisterBackgroundJob&lt;TJob&gt;()</c>.
/// </summary>
public interface IBackgroundJob
{
    /// <summary>A human-readable name for the job.</summary>
    string Name { get; }

    /// <summary>A short description of what the job does.</summary>
    string Description { get; }

    /// <summary>
    /// Executes the background job logic.
    /// </summary>
    Task ExecuteAsync();
}
