using System.Net;
using System.Text.RegularExpressions;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.Template.Entities;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.Template.Tests.IntegrationTests;

[TestClass]
public class SystemControllerTests
{
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public SystemControllerTests()
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

    private async Task<string> GetAntiCsrfToken(string url)
    {
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(html, @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" />");
        return match.Groups[1].Value;
    }

    private async Task LoginAsync(string email, string password)
    {
        var token = await GetAntiCsrfToken("/Account/Login");
        var loginContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", email },
            { "Password", password },
            { "__RequestVerificationToken", token }
        });
        var response = await _http.PostAsync("/Account/Login", loginContent);
        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
    }

    [TestMethod]
    public async Task TestIndex()
    {
        await LoginAsync("admin@default.com", "admin123");
        var response = await _http.GetAsync("/System/Index");
        response.EnsureSuccessStatusCode();
    }

    [TestMethod]
    public async Task TestShutdown()
    {
        await LoginAsync("admin@default.com", "admin123");
        var response = await _http.PostAsync("/System/Shutdown", null);
        Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
    }
}
