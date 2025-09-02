namespace Aiursoft.Template.Authorization;

/// <summary>
/// Defines all the claim types used in this application.
/// </summary>
public static class AppClaims
{
    public const string Type = "Permission";

    public static readonly string CanReadUsers = "CanReadUsers";
    public static readonly string CanDeleteUsers = "CanDeleteUsers";
    public static readonly string CanAddUser = "CanAddUser";
    public static readonly string CanEditUser = "CanEditUser";

    public static readonly string CanReadRoles = "CanReadRoles";
    public static readonly string CanDeleteRoles = "CanDeleteRoles";
    public static readonly string CanAddRole = "CanAddRole";
    public static readonly string CanAssignRoleToUser = "CanAssignRoleToUser";
}
