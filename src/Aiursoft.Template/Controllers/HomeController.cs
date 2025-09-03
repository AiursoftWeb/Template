using Aiursoft.Template.Models.HomeViewModels;
using Aiursoft.Template.Navigation;
using Aiursoft.Template.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Template.Controllers;

public class HomeController : Controller
{
    [RenderInNavBar(
        NavGroupName = "Home",
        CascadedLinksGroupName = "Dashboard",
        CascadedLinksIcon = "home",
        CascadedLinksOrder = 1,
        LinkText = "Main",
        LinkOrder = 1)]
    public IActionResult Index()
    {
        return this.StackView(new IndexViewModel());
    }
}
