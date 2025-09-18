using System.Net;
using System.Text.RegularExpressions;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.Template.Entities;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.Template.Tests.IntegrationTests;

[TestClass]
public class BasicTests
{
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public BasicTests()
    {
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            AllowAutoRedirect = false // 非常重要！禁止自动重定向，以便我们可以测试302响应
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
    [DataRow("/")]
    [DataRow("/hOmE?aaaaaa=bbbbbb")]
    [DataRow("/hOmE/InDeX")]
    public async Task GetHome(string url)
    {
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode(); // Status Code 200-299
    }

    private async Task<string> GetAntiCsrfToken(string url)
    {
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(html, @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" />");
        if (!match.Success)
        {
            throw new InvalidOperationException($"Could not find anti-CSRF token on page: {url}");
        }
        return match.Groups[1].Value;
    }

    [TestMethod]
    public async Task RegisterAndLoginAndLogOffTest()
    {
        var expectedUserName = $"test-{Guid.NewGuid()}";
        var email = $"{expectedUserName}@aiursoft.com";
        var password = "Test-Password-123";

        var registerToken = await GetAntiCsrfToken("/Account/Register");
        var registerContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Email", email },
            { "Password", password },
            { "ConfirmPassword", password },
            { "__RequestVerificationToken", registerToken }
        });

        var registerResponse = await _http.PostAsync("/Account/Register", registerContent);

        Assert.AreEqual(HttpStatusCode.Found, registerResponse.StatusCode);
        Assert.AreEqual("/", registerResponse.Headers.Location?.OriginalString);

        var homePageResponse = await _http.GetAsync("/Manage/Index");
        homePageResponse.EnsureSuccessStatusCode();

        var logOffToken = await GetAntiCsrfToken("/Manage/ChangePassword");
        var logOffContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "__RequestVerificationToken", logOffToken }
        });
        var logOffResponse = await _http.PostAsync("/Account/LogOff", logOffContent);

        Assert.AreEqual(HttpStatusCode.Found, logOffResponse.StatusCode);
        Assert.AreEqual("/", logOffResponse.Headers.Location?.OriginalString);

        var loginToken = await GetAntiCsrfToken("/Account/Login");
        var loginContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", email },
            { "Password", password },
            { "__RequestVerificationToken", loginToken }
        });

        var loginResponse = await _http.PostAsync("/Account/Login", loginContent);

        Assert.AreEqual(HttpStatusCode.Found, loginResponse.StatusCode);
        Assert.AreEqual("/", loginResponse.Headers.Location?.OriginalString);

        var finalHomePageResponse = await _http.GetAsync("/");
        finalHomePageResponse.EnsureSuccessStatusCode();
        var finalHtml = await finalHomePageResponse.Content.ReadAsStringAsync();
        Assert.IsTrue(finalHtml.Contains(expectedUserName));
    }

    [TestMethod]
    public async Task LoginWithInvalidCredentialsTest()
    {
        var email = $"test-{Guid.NewGuid()}@aiursoft.com";
        var password = "Wrong-Password-123";
        var loginToken = await GetAntiCsrfToken("/Account/Login");
        var loginContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EmailOrUserName", email },
            { "Password", password },
            { "__RequestVerificationToken", loginToken }
        });

        var loginResponse = await _http.PostAsync("/Account/Login", loginContent);
        Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);
        var html = await loginResponse.Content.ReadAsStringAsync();
        Assert.IsTrue(html.Contains("Invalid login attempt."));
    }
}
