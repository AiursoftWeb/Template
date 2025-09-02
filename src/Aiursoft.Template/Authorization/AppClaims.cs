namespace Aiursoft.Template.Authorization;

/// <summary>
/// A record to describe a permission in a structured way.
/// </summary>
/// <param name="Key">The programmatic key stored in the database (e.g., "CanReadUsers").</param>
/// <param name="Name">The user-friendly name displayed in the UI (e.g., "Read Users").</param>
/// <param name="Description">A detailed explanation of what the permission allows.</param>
public record PermissionDescriptor(string Key, string Name, string Description);

/// <summary>
/// Defines all the claim types used in this application.
/// </summary>
public static class AppClaims
{
    public const string Type = "Permission";

    public static readonly List<PermissionDescriptor> AllPermissions =
    [
        new("CanReadUsers",
            "Read Users",
            "Allows viewing the list of all users."),
        new(            "CanDeleteUsers",
            "Delete Users",
            "Allows the permanent deletion of user accounts."),
        new("CanAddUser",
            "Add New Users",
            "Grants permission to create new user accounts."),
        new("CanEditUser",
            "Edit User Information",
            "Allows modification of user details like email and roles. And also can reset user passwords."),
        new("CanReadRoles", "Read Roles",
            "Allows viewing the list of roles and their assigned permissions."),
        new("CanDeleteRoles", "Delete Roles", "Allows the permanent deletion of roles."),
        new("CanAddRole", "Add New Roles", "Grants permission to create new roles."),
        new("CanAssignRoleToUser", "Assign Roles to Users",
            "Allows assigning or removing roles for any user.")
    ];
}
