using System.Text.RegularExpressions;
using ReDocApp = ReDoc;

namespace Swashbuckle.AspNetCore.IntegrationTests;

[Collection("TestSite")]
public partial class VerifyTests(ITestOutputHelper outputHelper)
{
    [Theory]
    [InlineData(typeof(Basic.Startup), "/swagger/v1/swagger.json")]
    [InlineData(typeof(NSwagClientExample.Startup), "/swagger/v1/swagger.json")]
    [InlineData(typeof(CliExample.Startup), "/swagger/v1/swagger_net8.0.json")]
    [InlineData(typeof(ConfigFromFile.Startup), "/swagger/v1/swagger.json")]
    [InlineData(typeof(CustomDocumentSerializer.Startup), "/swagger/v1/swagger.json")]
    [InlineData(typeof(CustomUIConfig.Startup), "/swagger/v1/swagger.json")]
    [InlineData(typeof(CustomUIIndex.Startup), "/swagger/v1/swagger.json")]
    [InlineData(typeof(GenericControllers.Startup), "/swagger/v1/swagger.json")]
    [InlineData(typeof(MultipleVersions.Startup), "/swagger/1.0/swagger.json")]
    [InlineData(typeof(MultipleVersions.Startup), "/swagger/2.0/swagger.json")]
    [InlineData(typeof(OAuth2Integration.Startup), "/resource-server/swagger/v1/swagger.json")]
    [InlineData(typeof(ReDocApp.Startup), "/swagger/v1/swagger.json")]
    [InlineData(typeof(TestFirst.Startup), "/swagger/v1-generated/openapi.json")]
    public async Task SwaggerEndpoint_ReturnsValidSwaggerJson(
        Type startupType,
        string swaggerRequestUri)
    {
        var testSite = new TestSite(startupType, outputHelper);
        using var client = testSite.BuildClient();

        using var swaggerResponse = await client.GetAsync(swaggerRequestUri, TestContext.Current.CancellationToken);
        var swagger = await swaggerResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        await Verify(NormalizeLineBreaks(swagger))
            .UseDirectory("snapshots")
            .UseParameters(startupType, GetVersion(swaggerRequestUri))
            .UniqueForTargetFrameworkAndVersion();
    }

    [Fact]
    public async Task SwaggerEndpoint_ReturnsValidSwaggerJson_ForAutofaq()
    {
        var startupType = typeof(CliExampleWithFactory.Startup);
        const string swaggerRequestUri = "/swagger/v1/swagger_net8.0.json";

        var testSite = new TestSiteAutofaq(startupType);
        using var client = testSite.BuildClient();

        using var swaggerResponse = await client.GetAsync(swaggerRequestUri, TestContext.Current.CancellationToken);
        var swagger = await swaggerResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        await Verify(swagger)
            .UseDirectory("snapshots")
            .UseParameters(startupType, GetVersion(swaggerRequestUri))
            .UniqueForTargetFrameworkAndVersion();
    }

    [Theory]
    [InlineData("MinimalApp", "/swagger/v1/swagger.json")]
    [InlineData("TopLevelSwaggerDoc", "/swagger/v1.json")]
    [InlineData("MvcWithNullable", "/swagger/v1/swagger.json")]
    [InlineData("WebApi", "/swagger/v1/swagger.json")]
    [InlineData("WebApi.Aot", "/swagger/v1/swagger.json")]
    public async Task Swagger_IsValidJson_No_Startup(
        string assemblyName,
        string swaggerRequestUri)
    {
        var swaggerResponse = await SwaggerEndpointReturnsValidSwaggerJson(assemblyName, swaggerRequestUri);

        await Verify(swaggerResponse)
            .UseDirectory("snapshots")
            .UseParameters(assemblyName, GetVersion(swaggerRequestUri))
            .UniqueForTargetFrameworkAndVersion();
    }

    [Fact]
    public async Task TypesAreRenderedCorrectly()
    {
        using var client = SwaggerIntegrationTests.GetHttpClientForTestApplication("WebApi");

        var swaggerResponse = await SwaggerResponse(client, "/swagger/v1/swagger.json");

        await Verify(swaggerResponse)
            .UseDirectory("snapshots")
            .UniqueForTargetFrameworkAndVersion();
    }

    private static async Task<string> SwaggerEndpointReturnsValidSwaggerJson(string assemblyName, string swaggerRequestUri)
    {
        using var client = SwaggerIntegrationTests.GetHttpClientForTestApplication(assemblyName);
        return await SwaggerResponse(client, swaggerRequestUri);
    }

    private static async Task<string> SwaggerResponse(HttpClient client, string swaggerRequestUri)
    {
        using var swaggerResponse = await client.GetAsync(swaggerRequestUri);
        var contentStream = await swaggerResponse.Content.ReadAsStringAsync();
        return contentStream;
    }

    /// <summary>
    /// Normalize "\n" strings into "\r\n" which is expected linebreak in Verify verified.txt files.
    /// </summary>
    private static string NormalizeLineBreaks(string swagger)
        => UnixNewLineRegex().Replace(swagger, "\\r\\n");

    private static string GetVersion(string swaggerUi) =>
        VersionRegex().Match(swaggerUi).Groups[1].Value;

    [GeneratedRegex("/\\w+/([\\w+\\d+.-]+)/")]
    private static partial Regex VersionRegex();

    [GeneratedRegex(@"(?<!\\r)\\n")]
    private static partial Regex UnixNewLineRegex();
}
