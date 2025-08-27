using System.ComponentModel.DataAnnotations;
using Aiursoft.Template.Services;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.Template.Models.AccountViewModels;

public class LoginViewModel: UiStackLayoutViewModel
{
    // ReSharper disable once UnusedMember.Global
    // This constructor is used by the MVC framework.
    public LoginViewModel()
    {
    }

    public LoginViewModel(HttpContext context)
    {
        ViewModelArgsInjector.Inject(context, this, "Login");
    }

    [Required]
    [Display(Name ="Email or User name")]
    public string? EmailOrUserName { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name ="密码")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    public string? Password { get; set; }
}
