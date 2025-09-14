using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Aiursoft.Template.Entities;

public class User : IdentityUser
{
    [MaxLength(30)]
    [MinLength(2)]
    public required string DisplayName { get; set; }

    [MaxLength(150)]
    [MinLength(2)]
    public string AvatarRelativePath { get; set; } = "/node_modules/@aiursoft/uistack/dist/img/avatars/avatar.jpg";

    public DateTime CreationTime { get; init; } = DateTime.UtcNow;
}
