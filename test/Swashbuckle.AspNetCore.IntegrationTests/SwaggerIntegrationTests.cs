using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using ReDocApp = ReDoc;

namespace Swashbuckle.AspNetCore.IntegrationTests;

[Collection("TestSite")]
public class SwaggerIntegrationTests(ITestOutputHelper outputHelper)
{
    [Theory]
    [InlineData(typeof(Basic.Startup), "/swagger/v1/swagger.json")]
    [InlineData(typeof(CliExample.Startup), "/swagger/v1/swagger_net8.0.json")]
    [InlineData(typeof(ConfigFromFile.Startup), "/swagger/v1/swagger.json")]
    [InlineData(typeof(CustomUIConfig.Startup), "/swagger/v1/swagger.json")]
    [InlineData(typeof(CustomUIIndex.Startup), "/swagger/v1/swagger.json")]
    [InlineData(typeof(GenericControllers.Startup), "/swagger/v1/swagger.json")]
    [InlineData(typeof(MultipleVersions.Startup), "/swagger/1.0/swagger.json")]
    [InlineData(typeof(MultipleVersions.Startup), "/swagger/2.0/swagger.json")]
    [InlineData(typeof(NSwagClientExample.Startup), "/swagger/v1/swagger.json")]
    [InlineData(typeof(OAuth2Integration.Startup), "/resource-server/swagger/v1/swagger.json")]
    [InlineData(typeof(ReDocApp.Startup), "/swagger/v1/swagger.json")]
    [InlineData(typeof(TestFirst.Startup), "/swagger/v1-generated/openapi.json")]
    public async Task SwaggerEndpoint_ReturnsValidSwaggerJson(
        Type startupType,
        string swaggerRequestUri)
    {
        var testSite = new TestSite(startupType, outputHelper);
        using var client = testSite.BuildClient();

        await AssertValidSwaggerJson(client, swaggerRequestUri);
    }

    [Fact]
    public async Task SwaggerEndpoint_ReturnsValidSwaggerJson_ForAutofaq()
    {
        var testSite = new TestSiteAutofaq(typeof(CliExampleWithFactory.Startup));
        using var client = testSite.BuildClient();

        await AssertValidSwaggerJson(client, "/swagger/v1/swagger_net8.0.json");
    }

    [Fact]
    public async Task SwaggerEndpoint_ReturnsNotFound_IfUnknownSwaggerDocument()
    {
        var testSite = new TestSite(typeof(Basic.Startup), outputHelper);
        using var client = testSite.BuildClient();

        using var swaggerResponse = await client.GetAsync("/swagger/v2/swagger.json", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, swaggerResponse.StatusCode);
    }

    [Fact]
    public async Task SwaggerEndpoint_DoesNotReturnByteOrderMark()
    {
        var testSite = new TestSite(typeof(Basic.Startup), outputHelper);
        using var client = testSite.BuildClient();

        using var swaggerResponse = await client.GetAsync("/swagger/v1/swagger.json", TestContext.Current.CancellationToken);

        swaggerResponse.EnsureSuccessStatusCode();
        var contentBytes = await swaggerResponse.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
        var bomBytes = Encoding.UTF8.GetPreamble();
        Assert.NotEqual(bomBytes, contentBytes.Take(bomBytes.Length));
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("sv-SE")]
    public async Task SwaggerEndpoint_ReturnsCorrectPriceExample_ForDifferentCultures(string culture)
    {
        var testSite = new TestSite(typeof(Basic.Startup), outputHelper);
        using var client = testSite.BuildClient();

        using var swaggerResponse = await client.GetAsync($"/swagger/v1/swagger.json?culture={culture}", TestContext.Current.CancellationToken);

        swaggerResponse.EnsureSuccessStatusCode();
        using var contentStream = await swaggerResponse.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        var currentCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        try
        {
            var openApiDocument = await OpenApiDocumentLoader.LoadAsync(contentStream);
            var example = openApiDocument.Components.Schemas["Product"].Example;
            double price = example["price"].GetValue<double>();
            Assert.Equal(14.37, price);
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
        }
    }

    [Theory]
    [InlineData("/swagger/v1/swagger.json", "openapi", "3.0.4")]
    [InlineData("/swagger/v1/swaggerv2.json", "swagger", "2.0")]
    [InlineData("/swagger/v1/swaggerv3_1.json", "openapi", "3.1.1")]
    public async Task SwaggerMiddleware_CanBeConfiguredMultipleTimes(
        string swaggerUrl,
        string expectedVersionProperty,
        string expectedVersionValue)
    {
        using var client = new TestSite(typeof(Basic.Startup), outputHelper).BuildClient();

        using var response = await client.GetAsync(swaggerUrl, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        using var contentStream = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);

        var json = await JsonSerializer.DeserializeAsync<JsonElement>(contentStream, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expectedVersionValue, json.GetProperty(expectedVersionProperty).GetString());
    }

    [Theory]
    [InlineData("MinimalApp", "/swagger/v1/swagger.json")]
    [InlineData("TopLevelSwaggerDoc", "/swagger/v1.json")]
    [InlineData("MvcWithNullable", "/swagger/v1/swagger.json")]
    [InlineData("WebApi", "/swagger/v1/swagger.json")]
    [InlineData("WebApi.Aot", "/swagger/v1/swagger.json")]
    public async Task SwaggerEndpoint_ReturnsValidSwaggerJson_Without_Startup(
        string assemblyName,
        string swaggerRequestUri)
    {
        await SwaggerEndpointReturnsValidSwaggerJson(assemblyName, swaggerRequestUri);
    }

    [Fact]
    public async Task TypesAreRenderedCorrectly()
    {
        using var client = GetHttpClientForTestApplication("WebApi");

        using var response = await client.GetAsync("/swagger/v1/swagger.json", TestContext.Current.CancellationToken);

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.True(response.IsSuccessStatusCode, content);

        using var swaggerResponse = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken),
            cancellationToken: TestContext.Current.CancellationToken);

        var weatherForecast = swaggerResponse.RootElement
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty("WeatherForecast");

        Assert.Equal("object", weatherForecast.GetProperty("type").GetString());

        var properties = weatherForecast.GetProperty("properties");
        Assert.Equal(4, properties.EnumerateObject().Count());

        Assert.Multiple(
        [
            () => Assert.Equal("string", properties.GetProperty("date").GetProperty("type").GetString()),
            () => Assert.Equal("date", properties.GetProperty("date").GetProperty("format").GetString()),
            () => Assert.Equal("integer", properties.GetProperty("temperatureC").GetProperty("type").GetString()),
            () => Assert.Equal("int32", properties.GetProperty("temperatureC").GetProperty("format").GetString()),
            () => Assert.Equal("string", properties.GetProperty("summary").GetProperty("type").GetString()),
            () => Assert.True(properties.GetProperty("summary").GetProperty("nullable").GetBoolean()),
            () => Assert.Equal("integer", properties.GetProperty("temperatureF").GetProperty("type").GetString()),
            () => Assert.Equal("int32", properties.GetProperty("temperatureF").GetProperty("format").GetString()),
            () => Assert.True(properties.GetProperty("temperatureF").GetProperty("readOnly").GetBoolean()),
        ]);
    }

    private static async Task SwaggerEndpointReturnsValidSwaggerJson(string assemblyName, string swaggerRequestUri)
    {
        using var client = GetHttpClientForTestApplication(assemblyName);
        await AssertValidSwaggerJson(client, swaggerRequestUri);
    }

    internal static HttpClient GetHttpClientForTestApplication(string assemblyName)
    {
        var assembly = Assembly.Load(assemblyName);
        var entryPointType = assembly
            .GetTypes().FirstOrDefault(x => x.Name == "Program")
            ?? throw new InvalidOperationException($"The Program entry point was not found on assembly {assemblyName}.");

        var applicationType = typeof(TestApplication<>).MakeGenericType(entryPointType);
        var application = (IDisposable)Activator.CreateInstance(applicationType);
        Assert.NotNull(application);

        var createClientMethod = applicationType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(m => m.Name == "CreateDefaultClient" && m.GetParameters().Length == 1)
            ?? throw new InvalidOperationException($"The method CreateDefaultClient was not found on TestApplication<{assemblyName}.Program>.");

        // Pass null for DelegatingHandler[]
        var parameters = new object[] { null };

        var clientObject = (IDisposable)createClientMethod.Invoke(application, parameters);
        if (clientObject is not HttpClient client)
        {
            throw new InvalidOperationException($"The method CreateDefaultClient on TestApplication<{assemblyName}.Program> did not return a HttpClient.");
        }

        return client;
    }

    private static async Task AssertValidSwaggerJson(HttpClient client, string swaggerRequestUri)
    {
        using var swaggerResponse = await client.GetAsync(swaggerRequestUri);

        Assert.True(swaggerResponse.IsSuccessStatusCode, $"IsSuccessStatusCode is false. Response: '{await swaggerResponse.Content.ReadAsStringAsync()}'");
        using var contentStream = await swaggerResponse.Content.ReadAsStreamAsync();
        var (_, diagnostic) = await OpenApiDocumentLoader.LoadWithDiagnosticsAsync(contentStream);
        Assert.NotNull(diagnostic);
        Assert.Empty(diagnostic.Errors);
    }
}
