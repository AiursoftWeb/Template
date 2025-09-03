using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Template.Navigation;

public record NavLinkDefinition(string Href, string Text, int Order, string? RequiredPolicy);
public record NavItemDefinition(string UniqueId, string Text, string Icon, int Order, List<NavLinkDefinition> Links);
public record NavGroupDefinition(string Name, List<NavItemDefinition> Items);

public class NavigationState
{
    public readonly IReadOnlyList<NavGroupDefinition> NavMap;

    public NavigationState()
    {
        var navGroups = new Dictionary<string, NavGroupDefinition>();

        var controllers = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => typeof(Controller).IsAssignableFrom(type));

        foreach (var controller in controllers)
        {
            var controllerName = controller.Name.Replace("Controller", "");
            var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.IsDefined(typeof(RenderInNavBarAttribute), false));

            foreach (var method in methods)
            {
                var navAttr = method.GetCustomAttribute<RenderInNavBarAttribute>()!;
                var actionName = method.GetCustomAttribute<ActionNameAttribute>()?.Name ?? method.Name;

                var authorizeAttr = method.GetCustomAttribute<AuthorizeAttribute>();
                var requiredPolicy = authorizeAttr?.Policy;

                // 1. 找到或创建 NavGroup
                if (!navGroups.TryGetValue(navAttr.NavGroupName, out var group))
                {
                    group = new NavGroupDefinition(navAttr.NavGroupName, new List<NavItemDefinition>());
                    navGroups[navAttr.NavGroupName] = group;
                }

                // 2. 找到或创建 NavItem
                var item = group.Items.FirstOrDefault(i => i.Text == navAttr.CascadedLinksGroupName);
                if (item == null)
                {
                    item = new NavItemDefinition(navAttr.CascadedLinksGroupName.ToLower(), navAttr.CascadedLinksGroupName, navAttr.CascadedLinksIcon, navAttr.CascadedLinksOrder, new List<NavLinkDefinition>());
                    group.Items.Add(item);
                }

                // 3. 添加 NavLink
                item.Links.Add(new NavLinkDefinition(
                    Href: $"/{controllerName}/{actionName}",
                    Text: navAttr.LinkText,
                    Order: navAttr.LinkOrder,
                    RequiredPolicy: requiredPolicy));
            }
        }

        foreach (var group in navGroups.Values)
        {
            foreach (var item in group.Items)
            {
                item.Links.Sort((a, b) => a.Order.CompareTo(b.Order));
            }
            group.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        NavMap = navGroups.Values.ToList();
    }
}
