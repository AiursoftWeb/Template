using System.Net;
using System.Text.RegularExpressions;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.Template.Entities;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.Template.Tests.IntegrationTests;

[TestClass]
public class UsersControllerTests
{
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public UsersControllerTests()
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
    public async Task TestUsersWorkflow()
    {
        await LoginAsync("admin@default.com", "admin123");

        // 1. Index
        var indexResponse = await _http.GetAsync("/Users/Index");
        indexResponse.EnsureSuccessStatusCode();
        var indexHtml = await indexResponse.Content.ReadAsStringAsync();
        Assert.Contains("admin", indexHtml);

        // 2. Create
        var createToken = await GetAntiCsrfToken("/Users/Create");
        var userName = "testuser-" + Guid.NewGuid();
        var email = userName + "@aiursoft.com";
        var createContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "UserName", userName },
            { "DisplayName", "Test User" },
            { "Email", email },
            { "Password", "TestPassword123" },
            { "__RequestVerificationToken", createToken }
        });
        var createResponse = await _http.PostAsync("/Users/Create", createContent);
        Assert.AreEqual(HttpStatusCode.Found, createResponse.StatusCode);
        var userId = createResponse.Headers.Location?.OriginalString.Split("/").Last();
        Assert.IsNotNull(userId);

        // 3. Details
        var detailsResponse = await _http.GetAsync($"/Users/Details/{userId}");
        detailsResponse.EnsureSuccessStatusCode();
        var detailsHtml = await detailsResponse.Content.ReadAsStringAsync();
        Assert.Contains(userName, detailsHtml);

        // 4. Edit (GET)
        var editPageResponse = await _http.GetAsync($"/Users/Edit/{userId}");
        editPageResponse.EnsureSuccessStatusCode();

        // 5. Edit (POST)
        var editToken = await GetAntiCsrfToken($"/Users/Edit/{userId}");
        var newDisplayName = "Updated Test User";
        var editContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Id", userId },
            { "UserName", userName },
            { "Email", email },
            { "DisplayName", newDisplayName },
            { "AvatarUrl", User.DefaultAvatarPath },
            { "__RequestVerificationToken", editToken }
        });
        var editResponse = await _http.PostAsync($"/Users/Edit/{userId}", editContent);
        Assert.AreEqual(HttpStatusCode.Found, editResponse.StatusCode);

        // 6. ManageRoles (POST)
        var manageRolesToken = await GetAntiCsrfToken($"/Users/Edit/{userId}");
        var manageRolesContent = new Dictionary<string, string>
        {
            { "id", userId },
            { "__RequestVerificationToken", manageRolesToken },
            { "AllRoles[0].RoleName", "Administrators" },
            { "AllRoles[0].IsSelected", "true" }
        };
        var manageRolesResponse = await _http.PostAsync($"/Users/ManageRoles/{userId}", new FormUrlEncodedContent(manageRolesContent));
        Assert.AreEqual(HttpStatusCode.Found, manageRolesResponse.StatusCode);

        // 7. Delete (GET)
        var deletePageResponse = await _http.GetAsync($"/Users/Delete/{userId}");
        deletePageResponse.EnsureSuccessStatusCode();

        // 8. Delete (POST)
        var deleteToken = await GetAntiCsrfToken($"/Users/Delete/{userId}");
        var deleteContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "id", userId },
            { "__RequestVerificationToken", deleteToken }
        });
        var deleteResponse = await _http.PostAsync($"/Users/Delete/{userId}", deleteContent);
        Assert.AreEqual(HttpStatusCode.Found, deleteResponse.StatusCode);
    }

    [TestMethod]
    public async Task TestDetailsNotFound()
    {
        await LoginAsync("admin@default.com", "admin123");
        var response = await _http.GetAsync("/Users/Details/invalid-id");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task TestEditNotFound()
    {
        await LoginAsync("admin@default.com", "admin123");
        var response = await _http.GetAsync("/Users/Edit/invalid-id");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task TestDeleteNotFound()
    {
        await LoginAsync("admin@default.com", "admin123");
        var response = await _http.GetAsync("/Users/Delete/invalid-id");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
