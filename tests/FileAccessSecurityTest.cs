using Aiursoft.Template.Services.FileStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aiursoft.Template.Tests;

[TestClass]
public class FileAccessSecurityTest
{
    private StorageService _storageService = null!;
    private string _tempPath = null!;

    [TestInitialize]
    public void Initialize()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), "AiursoftTemplateTest_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempPath);

        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x["Storage:Path"]).Returns(_tempPath);

        _storageService = new StorageService(mockConfig.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, true);
        }
    }

    [TestMethod]
    public void TestGetFilePhysicalPath_NormalAccess()
    {
        var relativePath = "test.txt";
        var physicalPath = _storageService.GetFilePhysicalPath(relativePath);

        StringAssert.StartsWith(physicalPath, _tempPath);
        StringAssert.EndsWith(physicalPath, relativePath);
    }

    [TestMethod]
    [DataRow("../secret.txt")]
    [DataRow("../../etc/passwd")]
    [DataRow("/etc/passwd")]
    public void TestGetFilePhysicalPath_PathTraversal(string maliciousPath)
    {
        try
        {
            _storageService.GetFilePhysicalPath(maliciousPath);
            Assert.Fail("Expected ArgumentException was not thrown.");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    [TestMethod]
    public async Task TestSave_NormalAccess()
    {
        var mockFile = new Mock<IFormFile>();
        var content = "Hello World";
        var fileName = "test_upload.txt";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;

        mockFile.Setup(f => f.OpenReadStream()).Returns(ms);
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(ms.Length);
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, CancellationToken>((stream, token) =>
            {
                ms.Position = 0;
                ms.CopyTo(stream);
            })
            .Returns(Task.CompletedTask);

        var savedPath = await _storageService.Save("uploads/" + fileName, mockFile.Object);

        StringAssert.Contains(savedPath, "uploads");
        StringAssert.Contains(savedPath, fileName);
    }

    [TestMethod]
    [DataRow("../malicious.txt")]
    [DataRow("../../malicious.txt")]
    [DataRow("/absolute/path/malicious.txt")]
    public async Task TestSave_PathTraversal(string maliciousPath)
    {
        var mockFile = new Mock<IFormFile>();
        try
        {
            await _storageService.Save(maliciousPath, mockFile.Object);
            Assert.Fail("Expected ArgumentException was not thrown.");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }
}
