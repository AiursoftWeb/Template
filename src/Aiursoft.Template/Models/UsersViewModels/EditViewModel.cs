using System.ComponentModel.DataAnnotations;
using Aiursoft.CSTools.Attributes;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.Template.Models.UsersViewModels;

public class EditViewModel: UiStackLayoutViewModel
{
    public EditViewModel()
    {
        PageTitle = "Edit Teacher";
    }

    [Required]
    [Display(Name = "User name")]
    [ValidDomainName]
    public required string UserName { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public required string Email { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Reset Password (leave empty to keep the same password)")]
    public string? Password { get; set; }

    [Required]
    public required string Id { get; set; }

    [Display(Name = "Is Admin")]
    public bool IsAdmin { get; set; }
}
