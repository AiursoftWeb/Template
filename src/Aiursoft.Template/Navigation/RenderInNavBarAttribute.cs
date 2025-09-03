namespace Aiursoft.Template.Navigation;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class RenderInNavBarAttribute : Attribute
{
    /// <summary>
    /// The name of the top-level navigation group (e.g., "Admin").
    /// </summary>
    public required string NavGroupName { get; set; }

    /// <summary>
    /// The name of the cascaded sidebar item (e.g., "Directory").
    /// </summary>
    public required string CascadedLinksGroupName { get; set; }

    /// <summary>
    /// The text displayed for this specific link (e.g., "Roles").
    /// </summary>
    public required string LinkText { get; set; }

    /// <summary>
    /// The Lucide icon for the sidebar item. All links under the same ItemName should share the same icon.
    /// </summary>
    public string CascadedLinksIcon { get; set; } = "circle";

    /// <summary>
    /// The display order for the item within the group. Lower numbers appear first.
    /// </summary>
    public int CascadedLinksOrder { get; set; } = 100;

    /// <summary>
    /// The display order for the link within the item. Lower numbers appear first.
    /// </summary>
    public int LinkOrder { get; set; } = 100;

    public int NavGroupOrder { get; set; } = 100;
}
