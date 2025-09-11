using System.ComponentModel.DataAnnotations;
using Aiursoft.CSTools.Attributes;
using Aiursoft.UiStack.Layout;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Template.Models.UsersViewModels;

// Manage if a role is selected or not in the UI.
public class UserRoleViewModel
{
    public required string RoleName { get; set; }
    public bool IsSelected { get; set; }
}

public class EditViewModel : UiStackLayoutViewModel
{
    public EditViewModel()
    {
        PageTitle = "Edit User";
        AllRoles = [];
    }

    [Required(ErrorMessage = "The {0} is required.")]
    [Display(Name = "User name")]
    [ValidDomainName]
    public required string UserName { get; set; }

    [Required(ErrorMessage = "The {0} is required.")]
    [EmailAddress(ErrorMessage = "The {0} is not a valid email address.")]
    [Display(Name = "Email Address")]
    public required string Email { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Reset Password (leave empty to keep the same password)")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "The {0} is required.")]
    [FromRoute]
    public required string Id { get; set; }

    public List<UserRoleViewModel> AllRoles { get; set; }
}
