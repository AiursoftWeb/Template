using Aiursoft.Template.Authorization;
using Aiursoft.Template.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // 确保添加此 using 语句

namespace Aiursoft.Template;

public static class ProgramExtends
{
    public static async Task<IHost> SeedAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<TemplateDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        var role = await roleManager.FindByNameAsync("Admin");
        if (role == null)
        {
            role = new IdentityRole("Admin");
            await roleManager.CreateAsync(role);
        }

        var existingClaims = await roleManager.GetClaimsAsync(role);
        var existingClaimValues = existingClaims
            .Where(c => c.Type == AppPermissions.Type)
            .Select(c => c.Value)
            .ToHashSet();

        foreach (var permission in AppPermissions.AllPermissions)
        {
            if (!existingClaimValues.Contains(permission.Key))
            {
                var claim = new Claim(AppPermissions.Type, permission.Key);
                await roleManager.AddClaimAsync(role, claim);
            }
        }

        if (!await db.Users.AnyAsync(u => u.UserName == "admin"))
        {
            var user = new User
            {
                UserName = "admin",
                Email = "admin@default.com",
            };
            _ = await userManager.CreateAsync(user, "admin123");
            await userManager.AddToRoleAsync(user, "Admin");
        }
        return host;
    }
}
