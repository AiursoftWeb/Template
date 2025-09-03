using Aiursoft.Template.Models.HomeViewModels;
using Aiursoft.Template.Services;
using Aiursoft.UiStack.Navigation;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Template.Controllers;

public class HomeController : Controller
{
    [RenderInNavBar(
        NavGroupName = "Features",
        NavGroupOrder = 1,
        CascadedLinksGroupName = "Home",
        CascadedLinksIcon = "home",
        CascadedLinksOrder = 1,
        LinkText = "Index",
        LinkOrder = 1)]
    public IActionResult Index()
    {
        return this.StackView(new IndexViewModel());
    }
}
