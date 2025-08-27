using Aiursoft.Template.Services;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.Template.Models.ManageViewModels;

public class IndexViewModel: UiStackLayoutViewModel
{
    public IndexViewModel(HttpContext context)
    {
        ViewModelArgsInjector.Inject(context, this, "Manage");
    }
}
