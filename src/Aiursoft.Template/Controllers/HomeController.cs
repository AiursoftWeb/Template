using System.Diagnostics;
using Aiursoft.Template.Models;
using Aiursoft.Template.Models.HomeViewModels;
using Aiursoft.Template.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Template.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return this.StackView(new IndexViewModel());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return this.StackView(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
