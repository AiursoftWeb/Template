using Aiursoft.Template.Configuration;
using Aiursoft.Template.Entities;
using Aiursoft.UiStack.Layout;
using Aiursoft.UiStack.Navigation;
using Aiursoft.UiStack.Views.Shared.Components.FooterMenu;
using Aiursoft.UiStack.Views.Shared.Components.MegaMenu;
using Aiursoft.UiStack.Views.Shared.Components.Navbar;
using Aiursoft.UiStack.Views.Shared.Components.SideAdvertisement;
using Aiursoft.UiStack.Views.Shared.Components.Sidebar;
using Aiursoft.UiStack.Views.Shared.Components.SideLogo;
using Aiursoft.UiStack.Views.Shared.Components.SideMenu;
using Aiursoft.UiStack.Views.Shared.Components.UserDropdown;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Aiursoft.Template.Services;

public class ViewModelArgsInjector(
    NavigationState<Startup> navigationState,
    IAuthorizationService authorizationService,
    IOptions<AppSettings> appSettings,
    SignInManager<User> signInManager)
{
    public void Inject(
        HttpContext context,
        UiStackLayoutViewModel toInject)
    {
        toInject.AppName = "Template";
        toInject.Theme = UiTheme.Dark;
        toInject.SidebarTheme = UiSidebarTheme.Dark;
        toInject.Layout = UiLayout.Fluid;
        toInject.FooterMenu = new FooterMenuViewModel
        {
            AppBrand = new Link { Text = "Template", Href = "/" },
            Links =
            [
                new Link { Text = "Home", Href = "/" },
                new Link { Text = "Aiursoft", Href = "https://www.aiursoft.cn" },
            ]
        };
        toInject.Navbar = new NavbarViewModel();

        var currentViewingController = context.GetRouteValue("controller")?.ToString();
        var navGroupsForView = new List<NavGroup>();

        foreach (var groupDef in navigationState.NavMap)
        {
            var itemsForView = new List<CascadedSideBarItem>();
            foreach (var itemDef in groupDef.Items)
            {
                var linksForView = new List<CascadedLink>();
                foreach (var linkDef in itemDef.Links)
                {
                    bool isVisible;
                    if (string.IsNullOrEmpty(linkDef.RequiredPolicy))
                    {
                        isVisible = true;
                    }
                    else
                    {
                        var authResult = authorizationService.AuthorizeAsync(context.User, linkDef.RequiredPolicy).Result;
                        isVisible = authResult.Succeeded;
                    }

                    if (isVisible)
                    {
                        linksForView.Add(new CascadedLink { Href = linkDef.Href, Text = linkDef.Text });
                    }
                }

                if (linksForView.Any())
                {
                    itemsForView.Add(new CascadedSideBarItem
                    {
                        UniqueId = itemDef.UniqueId,
                        Text = itemDef.Text,
                        LucideIcon = itemDef.Icon,
                        IsActive = linksForView.Any(l => l.Href.StartsWith($"/{currentViewingController}")),
                        Links = linksForView
                    });
                }
            }

            if (itemsForView.Any())
            {
                navGroupsForView.Add(new NavGroup
                {
                    Name = groupDef.Name,
                    Items = itemsForView.Select(t => (SideBarItem)t).ToList()
                });
            }
        }

        toInject.Sidebar = new SidebarViewModel
        {
            SideLogo = new SideLogoViewModel
            {
                AppName = "Aiursoft Template",
                LogoUrl = "/logo.svg",
                Href = "/"
            },
            SideMenu = new SideMenuViewModel
            {
                Groups = navGroupsForView
            }
        };

        if (signInManager.IsSignedIn(context.User))
        {
            toInject.Navbar.UserDropdown = new UserDropdownViewModel
            {
                UserName = context.User.Identity?.Name ?? "Anonymous",
                UserAvatarUrl = "/node_modules/@aiursoft/uistack/dist/img/avatars/avatar.jpg",
                IconLinkGroups =
                [
                    new IconLinkGroup
                    {
                        Links =
                        [
                            new IconLink { Icon = "user", Text = "Profile", Href = "#" },
                            new IconLink { Icon = "pie-chart", Text = "Analytics", Href = "#" }
                        ]
                    },
                    new IconLinkGroup
                    {
                        Links =
                        [
                            new IconLink { Text = "Settings", Href = "/Manage", Icon = "settings" },
                            new IconLink { Text = "Help", Href = "#", Icon = "help-circle" },
                            new IconLink { Text = "Sign out", Href = "/Account/Logoff", Icon = "log-out" }
                        ]
                    }
                ]
            };
        }
        else
        {
            toInject.Sidebar.SideAdvertisement = new SideAdvertisementViewModel
            {
                Title = "Login",
                Description = "Login to get access to all features.",
                Href = "/Account/Login",
                ButtonText = "Login"
            };

            var allowRegister = appSettings.Value.Local.AllowRegister;
            var links = new List<IconLink>
            {
                new()
                {
                    Text = "Login",
                    Href = "/Account/Login",
                    Icon = "user"
                }
            };
            if (allowRegister)
            {
                links.Add(new IconLink
                {
                    Text = "Register", Href = "/Account/Register",
                    Icon = "user-plus"
                });
            }

            toInject.Navbar.UserDropdown = new UserDropdownViewModel
            {
                UserName = "Anonymous",
                UserAvatarUrl = "/node_modules/@aiursoft/uistack/dist/img/avatars/avatar.jpg",
                IconLinkGroups =
                [
                    new IconLinkGroup
                    {
                        Links = links.ToArray()
                    }
                ]
            };
        }
    }
}
