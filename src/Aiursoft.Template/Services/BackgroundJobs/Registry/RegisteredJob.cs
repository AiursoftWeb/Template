namespace Aiursoft.Template.Services.BackgroundJobs;

/// <summary>
/// Unified model for a registered background job.
/// During registration, only <see cref="JobType"/> is required.
/// For UI/API output, the registry also fills <see cref="Name"/> and <see cref="Description"/>.
/// </summary>
public class RegisteredJob
{
    /// <summary>The concrete type that implements <see cref="IBackgroundJob"/>.</summary>
    public required Type JobType { get; init; }

    /// <summary>A human-readable name for the job, resolved from <see cref="IBackgroundJob"/>.</summary>
    public string? Name { get; init; }

    /// <summary>A short description of what the job does, resolved from <see cref="IBackgroundJob"/>.</summary>
    public string? Description { get; init; }
}
