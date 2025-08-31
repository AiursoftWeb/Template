using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools.Switchable;
using Aiursoft.WebTools.Abstractions.Models;
using Aiursoft.Template.Entities;
using Aiursoft.Template.InMemory;
using Aiursoft.Template.MySql;
using Aiursoft.Template.Services;
using Aiursoft.Template.Sqlite;
using Aiursoft.UiStack.Layout;
using Microsoft.AspNetCore.Identity;

namespace Aiursoft.Template;

// {
//     "ConnectionStrings": {
//         "AllowCache": "True",
//
//         "DbType": "Sqlite",
//         "DefaultConnection": "DataSource=app.db;Cache=Shared"
//
//         // sudo docker run -d --name db -e MYSQL_RANDOM_ROOT_PASSWORD=true -e MYSQL_DATABASE=template -e MYSQL_USER=template -e MYSQL_PASSWORD=template_password -p 3306:3306 hub.aiursoft.cn/mysql
//         //"DbType": "MySql",
//         //"DefaultConnection": "Server=localhost;Database=template;Uid=template;Pwd=template_password;"
//     },
//     "AppSettings": {
//         "AuthProvider": "Local",
//         "OIDC": {
//             "Authority": "https://your-oidc-provider.com",
//             "ClientId": "your-client-id",
//             "ClientSecret": "your-client-secret"
//         },
//         "Local": {
//             "AllowRegister": true
//         }
//     },
//     "Logging": {
//         "LogLevel": {
//             "Default": "Information",
//             "Microsoft.AspNetCore": "Warning"
//         }
//     },
//     "AllowedHosts": "*"
// }
//

public class AppSettings
{
    public required string AuthProvider { get; init; } = "Local";
    public required OidcSettings OIDC { get; init; }
    public required LocalSettings Local { get; init; }
}

public class OidcSettings
{
    public required string Authority { get; init; } = "https://your-oidc-provider.com";
    public required string ClientId { get; init; } = "your-client-id";
    public required string ClientSecret { get; init; } = "your-client-secret";
}

public class LocalSettings
{
    public required bool AllowRegister { get; init; } = true;
}

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

        services.AddScoped<ViewModelArgsInjector>();

        services.AddControllersWithViews()
            .AddApplicationPart(typeof(Startup).Assembly)
            .AddApplicationPart(typeof(UiStackLayoutViewModel).Assembly);
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
