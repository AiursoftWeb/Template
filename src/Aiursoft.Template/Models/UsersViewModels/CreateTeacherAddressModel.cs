using System.ComponentModel.DataAnnotations;

namespace Aiursoft.Template.Models.UsersViewModels;

public class CreateTeacherAddressModel
{
    [EmailAddress]
    [Display(Name = "Email地址")]
    [Required]
    public required string Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "密码")]
    public required string Password { get; set; }
}
