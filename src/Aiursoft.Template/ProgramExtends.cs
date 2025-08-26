using Aiursoft.Template.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        if (!await db.Users.AnyAsync())
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
