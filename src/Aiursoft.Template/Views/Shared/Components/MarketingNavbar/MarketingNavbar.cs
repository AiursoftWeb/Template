using Microsoft.AspNetCore.Mvc;
using Aiursoft.Template.Services;

namespace Aiursoft.Template.Views.Shared.Components.MarketingNavbar;

public class MarketingNavbar(GlobalSettingsService globalSettingsService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(MarketingNavbarViewModel? model = null)
    {
        model ??= new MarketingNavbarViewModel();
        model.ProjectName = await globalSettingsService.GetSettingValueAsync("ProjectName");
        return View(model);
    }
}
