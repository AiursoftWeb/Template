using System.Diagnostics;
using Aiursoft.Template.Models;
using Aiursoft.Template.Models.HomeViewModels;
using Aiursoft.UiStack;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Template.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return this.UiStackView(new IndexViewModel(HttpContext));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return this.UiStackView(new ErrorViewModel(HttpContext) { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
