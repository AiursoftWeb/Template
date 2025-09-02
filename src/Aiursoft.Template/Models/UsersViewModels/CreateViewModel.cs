using System.ComponentModel.DataAnnotations;
using Aiursoft.CSTools.Attributes;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.Template.Models.UsersViewModels;

public class CreateViewModel: UiStackLayoutViewModel
{
    public CreateViewModel()
    {
        PageTitle = "Create User";
    }

    [Required]
    [Display(Name = "User name")]
    [ValidDomainName]
    public string? UserName { get; set; }

    [EmailAddress]
    [Display(Name = "Email Address")]
    [Required]
    public string? Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }
}
