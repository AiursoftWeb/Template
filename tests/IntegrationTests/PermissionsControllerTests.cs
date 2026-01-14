using System.Net;
using System.Text.RegularExpressions;
using Aiursoft.CSTools.Tools;
using Aiursoft.Template.Authorization;
using Aiursoft.DbTools;
using Aiursoft.Template.Entities;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.Template.Tests.IntegrationTests;

[TestClass]
public class PermissionsControllerTests
{
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public PermissionsControllerTests()
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
    public async Task GetIndex()
    {
        await LoginAsync("admin@default.com", "admin123");
        var url = "/Permissions/Index";
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Permissions", html);
    }

    [TestMethod]
    public async Task GetDetails()
    {
        await LoginAsync("admin@default.com", "admin123");
        var url = $"/Permissions/Details?key={AppPermissionNames.CanReadPermissions}";
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Permission Details", html);
    }

    [TestMethod]
    public async Task GetDetailsInvalidKey()
    {
        await LoginAsync("admin@default.com", "admin123");
        var url = "/Permissions/Details?key=invalid";
        var response = await _http.GetAsync(url);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetDetailsNullKey()
    {
        await LoginAsync("admin@default.com", "admin123");
        var url = "/Permissions/Details";
        var response = await _http.GetAsync(url);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task LoginAsync(string email, string password)
    {
        var loginPageResponse = await _http.GetAsync("/Account/Login");
        loginPageResponse.EnsureSuccessStatusCode();
        var html = await loginPageResponse.Content.ReadAsStringAsync();
        var match = Regex.Match(html, @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" />");
        var token = match.Groups[1].Value;

        var loginContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", email },
            { "Password", password },
            { "__RequestVerificationToken", token }
        });
        var response = await _http.PostAsync("/Account/Login", loginContent);
        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
    }
}
