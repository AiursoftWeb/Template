using System.Net;
using System.Text.RegularExpressions;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.Template.Entities;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.Template.Tests.IntegrationTests;

[TestClass]
public class RolesControllerTests
{
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public RolesControllerTests()
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
    public async Task TestRolesWorkflow()
    {
        await LoginAsync("admin@default.com", "admin123");

        // 1. Index
        var indexResponse = await _http.GetAsync("/Roles/Index");
        indexResponse.EnsureSuccessStatusCode();
        var indexHtml = await indexResponse.Content.ReadAsStringAsync();
        Assert.Contains("Administrators", indexHtml);

        // 2. Create
        var createToken = await GetAntiCsrfToken("/Roles/Create");
        var roleName = "TestRole-" + Guid.NewGuid();
        var createContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "RoleName", roleName },
            { "__RequestVerificationToken", createToken }
        });
        var createResponse = await _http.PostAsync("/Roles/Create", createContent);
        Assert.AreEqual(HttpStatusCode.Found, createResponse.StatusCode);
        var roleId = createResponse.Headers.Location?.OriginalString.Split("/").Last();
        Assert.IsNotNull(roleId);

        // 3. Details
        var detailsResponse = await _http.GetAsync($"/Roles/Details/{roleId}");
        detailsResponse.EnsureSuccessStatusCode();
        var detailsHtml = await detailsResponse.Content.ReadAsStringAsync();
        Assert.Contains(roleName, detailsHtml);

        // 4. Edit (GET)
        var editPageResponse = await _http.GetAsync($"/Roles/Edit/{roleId}");
        editPageResponse.EnsureSuccessStatusCode();

        // 5. Edit (POST)
        var editToken = await GetAntiCsrfToken($"/Roles/Edit/{roleId}");
        var newRoleName = roleName + "-Edited";
        var editContent = new Dictionary<string, string>
        {
            { "Id", roleId },
            { "RoleName", newRoleName },
            { "__RequestVerificationToken", editToken },
            { "Claims[0].Key", "CanReadPermissions" },
            { "Claims[0].Name", "CanReadPermissions" },
            { "Claims[0].Description", "CanReadPermissions" },
            { "Claims[0].IsSelected", "true" }
        };
        var editResponse = await _http.PostAsync($"/Roles/Edit/{roleId}", new FormUrlEncodedContent(editContent));
        Assert.AreEqual(HttpStatusCode.Found, editResponse.StatusCode);

        // Verify edit
        var detailsResponse2 = await _http.GetAsync($"/Roles/Details/{roleId}");
        var detailsHtml2 = await detailsResponse2.Content.ReadAsStringAsync();
        Assert.Contains(newRoleName, detailsHtml2);

        // 6. Delete (GET)
        var deletePageResponse = await _http.GetAsync($"/Roles/Delete/{roleId}");
        deletePageResponse.EnsureSuccessStatusCode();

        // 7. Delete (POST)
        var deleteToken = await GetAntiCsrfToken($"/Roles/Delete/{roleId}");
        var deleteContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "id", roleId },
            { "__RequestVerificationToken", deleteToken }
        });
        var deleteResponse = await _http.PostAsync($"/Roles/Delete/{roleId}", deleteContent);
        Assert.AreEqual(HttpStatusCode.Found, deleteResponse.StatusCode);

        // Verify delete
        var indexResponse2 = await _http.GetAsync("/Roles/Index");
        var indexHtml2 = await indexResponse2.Content.ReadAsStringAsync();
        Assert.DoesNotContain(newRoleName, indexHtml2);
    }

    [TestMethod]
    public async Task TestDetailsNotFound()
    {
        await LoginAsync("admin@default.com", "admin123");
        var response = await _http.GetAsync("/Roles/Details/invalid-id");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task TestEditNotFound()
    {
        await LoginAsync("admin@default.com", "admin123");
        var response = await _http.GetAsync("/Roles/Edit/invalid-id");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task TestDeleteNotFound()
    {
        await LoginAsync("admin@default.com", "admin123");
        var response = await _http.GetAsync("/Roles/Delete/invalid-id");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
