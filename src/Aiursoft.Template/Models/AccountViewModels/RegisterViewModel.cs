using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.Template.Models.AccountViewModels;

public class RegisterViewModel: UiStackLayoutViewModel
{
    public RegisterViewModel()
    {
        PageTitle = "Register";
    }

    [Required]
    [EmailAddress]
    [Display(Name = "Email地址（用于登录）")]
    public string? Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "密码")]
    public string? Password { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "确认密码")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }
}
