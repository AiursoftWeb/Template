namespace Aiursoft.Template.Authorization;

/// <summary>
/// A record to describe a permission in a structured way.
/// </summary>
/// <param name="Key">The programmatic key stored in the database (e.g., "CanReadUsers").</param>
/// <param name="Name">The user-friendly name displayed in the UI (e.g., "Read Users").</param>
/// <param name="Description">A detailed explanation of what the permission allows.</param>
public record PermissionDescriptor(string Key, string Name, string Description);

public static class AppPermissions
{
    public const string Type = "Permission";

    public static readonly List<PermissionDescriptor> AllPermissions =
    [
        // All keys are now referenced from the single source of truth.
        new(AppPermissionNames.CanReadUsers,
            "Read Users",
            "Allows viewing the list of all users."),
        new(AppPermissionNames.CanDeleteUsers,
            "Delete Users",
            "Allows the permanent deletion of user accounts."),
        new(AppPermissionNames.CanAddUsers,
            "Add New Users",
            "Grants permission to create new user accounts."),
        new(AppPermissionNames.CanEditUsers,
            "Edit User Information",
            "Allows modification of user details like email and roles, and can also reset user passwords."),
        new(AppPermissionNames.CanReadRoles,
            "Read Roles",
            "Allows viewing the list of roles and their assigned permissions."),
        new(AppPermissionNames.CanDeleteRoles,
            "Delete Roles",
            "Allows the permanent deletion of roles."),
        new(AppPermissionNames.CanAddRoles,
            "Add New Roles",
            "Grants permission to create new roles."),
        new(AppPermissionNames.CanEditRoles,
            "Edit Role Information",
            "Allows modification of role names and their assigned permissions."),
        new(AppPermissionNames.CanAssignRoleToUser,
            "Assign Roles to Users",
            "Allows assigning or removing roles for any user.")
    ];
}
