using Aiursoft.Template.Authorization;
using Aiursoft.Template.Services;
using Aiursoft.UiStack.Navigation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Aiursoft.Template.Models.SystemViewModels;

namespace Aiursoft.Template.Controllers;

[Authorize]
public class SystemController : Controller
{
    [Authorize(Policy = AppPermissionNames.CanViewSystemContext)]
    [RenderInNavBar(
        NavGroupName = "Admin",
        NavGroupOrder = 9999,
        CascadedLinksGroupName = "System",
        CascadedLinksIcon = "cog",
        CascadedLinksOrder = 1,
        LinkText = "Info",
        LinkOrder = 1)]
    public IActionResult Index()
    {
        return this.StackView(new IndexViewModel());
    }
}
