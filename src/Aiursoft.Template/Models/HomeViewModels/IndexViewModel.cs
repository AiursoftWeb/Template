using Aiursoft.Template.Services;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.Template.Models.HomeViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel(HttpContext context)
    {
        ViewModelArgsInjector.Inject(context, this, "Index");
    }
}
