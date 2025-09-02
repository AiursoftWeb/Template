using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.Template.Models.RolesViewModels;

public class RoleClaimViewModel
{
    public required string ClaimType { get; set; }
    public bool IsSelected { get; set; }
}

public class EditViewModel: UiStackLayoutViewModel
{
    public EditViewModel()
    {
        PageTitle = "Edit Role";
        Claims = new List<RoleClaimViewModel>();
    }

    [Required]
    public required string Id { get; set; }

    [Required]
    [Display(Name = "Role Name")]
    public required string RoleName { get; set; }

    public List<RoleClaimViewModel> Claims { get; set; }
}
