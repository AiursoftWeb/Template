using Aiursoft.Template.Authorization;
using Aiursoft.Template.Entities;
using Aiursoft.UiStack.Layout;
using Microsoft.AspNetCore.Identity;

namespace Aiursoft.Template.Models.PermissionsViewModels;

public class DetailsViewModel : UiStackLayoutViewModel
{
    public DetailsViewModel()
    {
        PageTitle = "Permission Details";
    }

    public required PermissionDescriptor Permission { get; set; }

    public required List<IdentityRole> Roles { get; set; }

    public required List<User> Users { get; set; }
}
