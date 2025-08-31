using Aiursoft.Template.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.Template.Models.UsersViewModels;

public class DetailsViewModel : UiStackLayoutViewModel
{
    public DetailsViewModel()
    {
        PageTitle = "User Details";
    }

    public required User User { get; set; }
}
