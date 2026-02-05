using Aiursoft.Template.Models.HomeViewModels;
using Aiursoft.Template.Services;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Template.Controllers;

[LimitPerMin]
public class HomeController : Controller
{
    public async Task<IActionResult> Index()
    {
        return await this.SimpleViewAsync(new IndexViewModel());
    }
}
