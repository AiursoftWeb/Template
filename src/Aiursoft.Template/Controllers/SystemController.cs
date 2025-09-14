using Aiursoft.Template.Authorization;
using Aiursoft.Template.Services;
using Aiursoft.UiStack.Navigation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Aiursoft.Template.Models.SystemViewModels;

namespace Aiursoft.Template.Controllers;

/// <summary>
/// This controller is used to handle system related actions like shutdown.
/// </summary>
[Authorize]
public class SystemController(ILogger<SystemController> logger) : Controller
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

    [HttpPost]
    [Authorize(Policy = AppPermissionNames.CanRebootThisApp)] // Use the specific permission
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult Shutdown([FromServices] IHostApplicationLifetime appLifetime)
    {
        logger.LogWarning("Application shutdown was requested by user: '{UserName}'", User.Identity?.Name);
        appLifetime.StopApplication();
        return Accepted();
    }
}
