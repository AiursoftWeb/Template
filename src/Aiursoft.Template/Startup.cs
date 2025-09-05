using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools.Switchable;
using Aiursoft.Template.Authorization;
using Aiursoft.Template.Configuration;
using Aiursoft.WebTools.Abstractions.Models;
using Aiursoft.Template.Entities;
using Aiursoft.Template.InMemory;
using Aiursoft.Template.MySql;
using Aiursoft.Template.Services;
using Aiursoft.Template.Sqlite;
using Aiursoft.UiStack.Layout;
using Aiursoft.UiStack.Navigation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Aiursoft.Template;

public class Startup : IWebStartup
{
    public void ConfigureServices(IConfiguration configuration, IWebHostEnvironment environment, IServiceCollection services)
    {
        // AppSettings.
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

        // Relational database
        var (connectionString, dbType, allowCache) = configuration.GetDbSettings();
        services.AddSwitchableRelationalDatabase(
            dbType: EntryExtends.IsInUnitTests() ? "InMemory": dbType,
            connectionString: connectionString,
            supportedDbs:
            [
                new MySqlSupportedDb(allowCache: allowCache, splitQuery: false),
                new SqliteSupportedDb(allowCache: allowCache, splitQuery: true),
                new InMemorySupportedDb()
            ]);

        services.AddMemoryCache();
        services.AddIdentity<User, IdentityRole>(options => options.Password = new PasswordOptions
        {
            RequireNonAlphanumeric = false,
            RequireDigit = false,
            RequiredLength = 6,
            RequiredUniqueChars = 0,
            RequireLowercase = false,
            RequireUppercase = false
        })
        .AddEntityFrameworkStores<TemplateDbContext>()
        .AddDefaultTokenProviders();

        services.AddAuthorization(options =>
        {
            foreach (var permission in AppPermissions.AllPermissions)
            {
                options.AddPolicy(
                    name: permission.Key,
                    policy => policy.RequireClaim(AppPermissions.Type, permission.Key));
            }
        });

        services.AddSingleton<NavigationState<Startup>>();
        services.AddScoped<ViewModelArgsInjector>();
        services.AddControllersWithViews()
            .AddApplicationPart(typeof(Startup).Assembly)
            .AddApplicationPart(typeof(UiStackLayoutViewModel).Assembly)
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization();
    }

    public void Configure(WebApplication app)
    {
        app.UseExceptionHandler("/Error/Error");
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapDefaultControllerRoute();
    }
}
