// ... other using statements

using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.Template.Models.RolesViewModels;

public class RoleClaimViewModel
{
    public required string Key { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public bool IsSelected { get; set; }
}

public class EditViewModel: UiStackLayoutViewModel
{
    // No changes needed here, as it already contains the List<RoleClaimViewModel>.
    public EditViewModel()
    {
        PageTitle = "Edit Role";
        Claims = [];
    }

    [Required]
    public required string Id { get; set; }

    [Required]
    [Display(Name = "Role Name")]
    public required string RoleName { get; set; }

    public List<RoleClaimViewModel> Claims { get; set; }
}
