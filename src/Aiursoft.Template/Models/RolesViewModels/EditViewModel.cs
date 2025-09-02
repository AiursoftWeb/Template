using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.Template.Models.RolesViewModels;

public class EditViewModel: UiStackLayoutViewModel
{
    public EditViewModel()
    {
        PageTitle = "Edit Role";
    }

    [Required]
    public required string Id { get; set; }

    [Required]
    [Display(Name = "Role Name")]
    public required string RoleName { get; set; }
}
