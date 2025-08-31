using System.Diagnostics;
using Aiursoft.Template.Models;
using Aiursoft.Template.Models.HomeViewModels;
using Aiursoft.Template.Services;
using Aiursoft.UiStack.Layout;
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

public static class Extensions
{
    public static ViewResult StackView(this Controller controller, UiStackLayoutViewModel model)
    {
        var services = controller.HttpContext.RequestServices;
        var injector = services.GetRequiredService<ViewModelArgsInjector>();
        injector.Inject(controller.HttpContext, model);
        return controller.View(model);
    }

    public static ViewResult StackView(
        this Controller controller,
        UiStackLayoutViewModel model,
        string viewName)
    {
        var services = controller.HttpContext.RequestServices;
        var injector = services.GetRequiredService<ViewModelArgsInjector>();
        injector.Inject(controller.HttpContext, model);
        return controller.View(model);
    }
}
