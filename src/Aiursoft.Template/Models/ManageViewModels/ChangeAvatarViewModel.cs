using System.ComponentModel.DataAnnotations;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.Template.Models.ManageViewModels;

public class ChangeAvatarViewModel : UiStackLayoutViewModel
{
    public ChangeAvatarViewModel()
    {
        PageTitle = "Change Avatar";
    }

    [Display(Name = "Avatar file")]
    [Required(ErrorMessage = "The avatar file is required.")]
    [RegularExpression(@"^avatar.*", ErrorMessage = "The avatar file is invalid. Please upload it again.")]
    public string? AvatarUrl { get; set; }
}
