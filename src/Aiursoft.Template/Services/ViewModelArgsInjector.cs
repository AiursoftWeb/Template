using Aiursoft.UiStack.Layout;
using Aiursoft.UiStack.Views.Shared.Components.FooterMenu;
using Aiursoft.UiStack.Views.Shared.Components.MegaMenu;
using Aiursoft.UiStack.Views.Shared.Components.SideAdvertisement;
using Aiursoft.UiStack.Views.Shared.Components.Sidebar;
using Aiursoft.UiStack.Views.Shared.Components.SideLogo;
using Aiursoft.UiStack.Views.Shared.Components.SideMenu;

namespace Aiursoft.Template.Services;

public static class ViewModelArgsInjector
{
    public static void Inject(
        HttpContext context,
        UiStackLayoutViewModel toInject,
        string pageTitle)
    {
        toInject.PageTitle = pageTitle;
        toInject.AppName = "Template";
        toInject.Theme = UiTheme.Dark;
        toInject.SidebarTheme = UiSidebarTheme.Dark;
        toInject.Layout = UiLayout.Fluid;
        toInject.FooterMenu = new FooterMenuViewModel
        {
            AppBrand = new Link { Text = "ManHours", Href = "/" },
            Links =
            [
                new Link { Text = "Home", Href = "/" },
                new Link { Text = "Aiursoft", Href = "https://www.aiursoft.cn" },
            ]
        };
        toInject.Sidebar = new SidebarViewModel
        {
            SideLogo = new SideLogoViewModel
            {
                AppName = "Aiursoft UI Stack",
                LogoUrl = "https://docs.anduinos.com/Assets/logo.svg",
                Href = "/"
            },
            SideMenu = new SideMenuViewModel
            {
                Groups =
                [
                    new NavGroup
                    {
                        Name = "Home",
                        Items =
                        [
                            new CascadedSideBarItem
                            {
                                UniqueId = "dashboards",
                                Text = "Dashboards",
                                IsActive = true,
                                LucideIcon = "sliders",
                                Decoration = new Decoration
                                {
                                    Text = "5",
                                    ColorClass = "primary"
                                },
                                Links =
                                [
                                    new CascadedLink
                                    {
                                        Href = "/",
                                        Text = "Default",
                                        IsActive = true
                                    }
                                ]
                            }
                        ]
                    },
                    new NavGroup
                    {
                        Name = "Admin",
                        Items =
                        [
                            new CascadedSideBarItem
                            {
                                UniqueId = "Manage",
                                Text = "Manage",
                                LucideIcon = "layout",
                                Links =
                                [
                                    new CascadedLink { Href = "/Sites", Text = "Sites" },
                                    new CascadedLink { Href = "/Users", Text = "Users" }
                                ]
                            },
                        ]
                    }
                ]
            }
        };

        if (!context.User.Identity?.IsAuthenticated == true)
        {
            toInject.Sidebar.SideAdvertisement = new SideAdvertisementViewModel
            {
                Title = "Login",
                Description = "Login to get access to all features.",
                Href = "/Account/Login",
                ButtonText = "Login"
            };
        }
    }
}
