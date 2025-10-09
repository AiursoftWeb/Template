using System.Diagnostics;
using Aiursoft.Template.Models.ErrorViewModels;
using Aiursoft.Template.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Template.Controllers;

/// <summary>
/// This controller is used to show error pages.
/// </summary>
public class ErrorController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return this.StackView(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Route("Error/Unauthorized")]
    public IActionResult UnauthorizedPage()
    {
        return this.StackView(new UnauthorizedViewModel(), viewName: "Unauthorized");
    }
}
