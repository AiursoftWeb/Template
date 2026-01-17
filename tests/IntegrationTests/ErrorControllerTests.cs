namespace Aiursoft.Template.Tests.IntegrationTests;

[TestClass]
public class ErrorControllerTests : TestBase
{
    [TestMethod]
    public async Task GetError()
    {
        var url = "/Error/Error";
        var response = await Http.GetAsync(url);
        response.EnsureSuccessStatusCode();
    }

    [TestMethod]
    public async Task GetUnauthorized()
    {
        var url = "/Error/Unauthorized?returnUrl=/dashboard";
        var response = await Http.GetAsync(url);
        response.EnsureSuccessStatusCode();
    }

    [TestMethod]
    public async Task GetBadRequest()
    {
        var url = "/Error/BadRequestPage";
        var response = await Http.GetAsync(url);
        response.EnsureSuccessStatusCode();
    }

    [TestMethod]
    public async Task GetCode400()
    {
        var url = "/Error/Code400";
        var response = await Http.GetAsync(url);
        response.EnsureSuccessStatusCode();
    }
}
