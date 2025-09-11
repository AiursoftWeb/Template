// In Services/CustomClaimsPrincipalFactory.cs
using Aiursoft.Template.Entities; // Your User class namespace
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Aiursoft.Template.Services;

public class TemplateClaimsPrincipalFactory(
    RoleManager<IdentityRole> roleManager,
    UserManager<User> userManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<User, IdentityRole>(userManager, roleManager, optionsAccessor)
{
    public static string DisplayNameClaimType = "DisplayName";

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        if (!string.IsNullOrWhiteSpace(user.DisplayName))
        {
            identity.AddClaim(new Claim(DisplayNameClaimType, user.DisplayName));
        }
        return identity;
    }
}
