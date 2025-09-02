using Aiursoft.Template.Entities;
using Aiursoft.UiStack.Layout;
using System.Collections.Generic;

namespace Aiursoft.Template.Models.UsersViewModels;

// User with roles view model.
public class UserWithRolesViewModel
{
    public required User User { get; set; }
    public required IList<string> Roles { get; set; }
}

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel()
    {
        PageTitle = "Users";
    }

    public required List<UserWithRolesViewModel> Users { get; set; }
}
