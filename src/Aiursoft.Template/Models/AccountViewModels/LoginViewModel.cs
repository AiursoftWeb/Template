using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Template.Models.AccountViewModels;

public class LoginViewModel
{
    [Required]
    [Display(Name ="Email or UserName")]
    public required string EmailOrUserName { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name ="密码")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    public required  string Password { get; set; }
}
