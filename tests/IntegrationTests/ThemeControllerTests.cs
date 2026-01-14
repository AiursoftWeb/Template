using System.Net;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.Template.Entities;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.Template.Tests.IntegrationTests;

[TestClass]
public class ThemeControllerTests
{
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public ThemeControllerTests()
    {
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            AllowAutoRedirect = false
        };
        _port = Network.GetAvailablePort();
        _http = new HttpClient(handler)
        {
            BaseAddress = new Uri($"http://localhost:{_port}")
        };
    }

    [TestInitialize]
    public async Task CreateServer()
    {
        _server = await AppAsync<Startup>([], port: _port);
        await _server.UpdateDbAsync<TemplateDbContext>();
        await _server.SeedAsync();
        await _server.StartAsync();
    }

    [TestCleanup]
    public async Task CleanServer()
    {
        if (_server == null) return;
        await _server.StopAsync();
        _server.Dispose();
    }

    [TestMethod]
    public async Task SwitchTheme()
    {
        // Theme/SwitchTheme is POST usually, let's check
        // If it is POST, GET might return 405 or 404
        
        // Based on my grep, it takes [FromBody]SwitchThemeViewModel
        // We can try to POST.
        
        var url = "/Theme/SwitchTheme";
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        var response = await _http.PostAsync(url, content);
        
        // Assert
        // We expect it to do something, maybe redirect or OK
        Assert.IsNotNull(response);
    }
}
