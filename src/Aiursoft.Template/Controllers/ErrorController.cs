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
        return this.StackView(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }, viewName: "Error");
    }

    [Route("Error/Code{code}")]
    public IActionResult Code(int code)
    {
        if (code == 400)
        {
            return BadRequestPage();
        }

        return Error();
    }

    public IActionResult BadRequestPage()
    {
        return this.StackView(new BadRequestViewModel(), viewName: "BadRequest");
    }

    [Route("Error/Unauthorized")]
    [HttpGet]
    public IActionResult UnauthorizedPage([FromQuery]string returnUrl = "/")
    {
        if (!Url.IsLocalUrl(returnUrl))
        {
            returnUrl = "/";
        }

        return this.StackView(new UnauthorizedViewModel
        {
            ReturnUrl = returnUrl
        }, viewName: "Unauthorized");
    }
}
