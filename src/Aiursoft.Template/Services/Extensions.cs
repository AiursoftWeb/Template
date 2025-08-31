using Aiursoft.UiStack.Layout;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Template.Services;

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
