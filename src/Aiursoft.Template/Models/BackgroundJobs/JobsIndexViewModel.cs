using Aiursoft.Template.Services.BackgroundJobs;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.Template.Models.BackgroundJobs;

public class JobsIndexViewModel : UiStackLayoutViewModel
{
    public IReadOnlyList<RegisteredJob> RegisteredJobs { get; init; } = [];
    public IReadOnlyList<ScheduledTaskRegistration> ScheduledTasks { get; init; } = [];
    public IReadOnlyDictionary<Type, DateTime> LastRunAtByJobType { get; init; } =
        new Dictionary<Type, DateTime>();
    public IEnumerable<JobInfo> AllRecentJobs { get; init; } = [];
}
