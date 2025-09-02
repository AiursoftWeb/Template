using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.Template.Models.RolesViewModels;

public class CreateViewModel: UiStackLayoutViewModel
{
    public CreateViewModel()
    {
        PageTitle = "Create Role";
    }

    [Required]
    [Display(Name = "Role Name")]
    public string? RoleName { get; set; }
}
