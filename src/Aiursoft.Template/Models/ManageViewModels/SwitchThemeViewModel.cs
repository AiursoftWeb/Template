using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Template.Models.ManageViewModels;

public class SwitchThemeViewModel
{
    [Required]
    public required string Theme { get; set; }
}
