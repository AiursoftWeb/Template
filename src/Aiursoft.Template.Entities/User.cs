using Microsoft.AspNetCore.Identity;

namespace Aiursoft.Template.Entities;

public class User : IdentityUser
{
    public DateTime CreationTime { get; init; } = DateTime.UtcNow;
}
