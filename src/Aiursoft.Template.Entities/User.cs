using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Aiursoft.Template.Entities;

public class User : IdentityUser
{
    [MaxLength(30)]
    [MinLength(2)]
    public required string DisplayName { get; set; }

    public required bool PreferDarkTheme { get; set; } = false;
    public DateTime CreationTime { get; init; } = DateTime.UtcNow;
}
