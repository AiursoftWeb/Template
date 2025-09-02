using Aiursoft.UiStack.Layout;
using Microsoft.AspNetCore.Identity;

namespace Aiursoft.Template.Models.RolesViewModels;

public class DetailsViewModel : UiStackLayoutViewModel
{
    public DetailsViewModel()
    {
        PageTitle = "Role Details";
    }

    public required IdentityRole Role { get; set; }
}
