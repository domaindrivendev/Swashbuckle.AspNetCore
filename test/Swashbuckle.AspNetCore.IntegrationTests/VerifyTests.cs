using System.Text.RegularExpressions;
using ReDocApp = ReDoc;

namespace Swashbuckle.AspNetCore.IntegrationTests;

[Collection("TestSite")]
public partial class VerifyTests
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
        var testSite = new TestSite(startupType);
        using var client = testSite.BuildClient();

        using var swaggerResponse = await client.GetAsync(swaggerRequestUri);
        var swagger = await swaggerResponse.Content.ReadAsStringAsync();

        await Verifier.Verify(NormalizeLineBreaks(swagger))
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

        using var swaggerResponse = await client.GetAsync(swaggerRequestUri);
        var swagger = await swaggerResponse.Content.ReadAsStringAsync();

        await Verifier.Verify(swagger)
            .UseDirectory("snapshots")
            .UseParameters(startupType, GetVersion(swaggerRequestUri))
            .UniqueForTargetFrameworkAndVersion();
    }

    [Theory]
    [InlineData(typeof(MinimalApp.Program), "/swagger/v1/swagger.json")]
    [InlineData(typeof(TopLevelSwaggerDoc.Program), "/swagger/v1.json")]
    [InlineData(typeof(MvcWithNullable.Program), "/swagger/v1/swagger.json")]
    [InlineData(typeof(WebApi.Program), "/swagger/v1/swagger.json")]
    [InlineData(typeof(WebApi.Aot.Program), "/swagger/v1/swagger.json")]
    public async Task Swagger_IsValidJson_No_Startup(
        Type entryPointType,
        string swaggerRequestUri)
    {
        var swaggerResponse = await SwaggerEndpointReturnsValidSwaggerJson(entryPointType, swaggerRequestUri);

        await Verifier.Verify(swaggerResponse)
            .UseDirectory("snapshots")
            .UseParameters(entryPointType, GetVersion(swaggerRequestUri))
            .UniqueForTargetFrameworkAndVersion();
    }

    [Fact]
    public async Task TypesAreRenderedCorrectly()
    {
        using var application = new TestApplication<WebApi.Program>();
        using var client = application.CreateDefaultClient();

        var swaggerResponse = await SwaggerResponse(client, "/swagger/v1/swagger.json");

        await Verifier.Verify(swaggerResponse)
            .UseDirectory("snapshots")
            .UniqueForTargetFrameworkAndVersion();
    }

    private static async Task<string> SwaggerEndpointReturnsValidSwaggerJson(Type entryPointType, string swaggerRequestUri)
    {
        using var client = SwaggerIntegrationTests.GetHttpClientForTestApplication(entryPointType);
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
