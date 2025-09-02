using Aiursoft.Template.Authorization;
using Aiursoft.Template.Entities;
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

    public required List<PermissionDescriptor> Permissions { get; set; }

    public required IList<User> UsersInRole { get; set; }
}
