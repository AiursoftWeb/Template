using Aiursoft.Template.Services;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.Template.Models;

public class ErrorViewModel: UiStackLayoutViewModel
{
    public ErrorViewModel(HttpContext context)
    {
        ViewModelArgsInjector.Inject(context, this, "Error");
    }

    public required string RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
