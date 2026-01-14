using System.Net;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.Template.Entities;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.Template.Tests.IntegrationTests;

[TestClass]
public class FilesControllerTests
{
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public FilesControllerTests()
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
    public async Task TestUploadAndDownload()
    {
        // 1. Upload
        var content = new StringContent("Hello World");
        var multipartContent = new MultipartFormDataContent();
        multipartContent.Add(content, "file", "test.txt");

        var uploadResponse = await _http.PostAsync("/upload/test", multipartContent);
        uploadResponse.EnsureSuccessStatusCode();
        var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<UploadResult>();
        Assert.IsNotNull(uploadResult);
        Assert.IsNotNull(uploadResult.Path);

        // 2. Download
        var downloadResponse = await _http.GetAsync("/download/" + uploadResult.Path);
        downloadResponse.EnsureSuccessStatusCode();
        var downloadContent = await downloadResponse.Content.ReadAsStringAsync();
        Assert.AreEqual("Hello World", downloadContent);
    }

    [TestMethod]
    public async Task TestUploadInvalidFileName()
    {
        var content = new StringContent("Hello World");
        var multipartContent = new MultipartFormDataContent();
        multipartContent.Add(content, "file", "../test.txt");

        var uploadResponse = await _http.PostAsync("/upload/test", multipartContent);
        Assert.AreEqual(HttpStatusCode.BadRequest, uploadResponse.StatusCode);
    }

    [TestMethod]
    public async Task TestDownloadNotFound()
    {
        var downloadResponse = await _http.GetAsync("/download/non-existing.txt");
        Assert.AreEqual(HttpStatusCode.NotFound, downloadResponse.StatusCode);
    }

    private class UploadResult
    {
        public string Path { get; init; } = string.Empty;
        public string InternetPath { get; init; } = string.Empty;
    }
}
