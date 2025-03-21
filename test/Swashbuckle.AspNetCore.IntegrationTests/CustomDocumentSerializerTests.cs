using System.Text;
using System.Text.Json;
using Microsoft.Extensions.ApiDescriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.IntegrationTests;

[Collection("TestSite")]
public class CustomDocumentSerializerTests
{
    [Fact]
    public async Task TestSite_Writes_Custom_V3_Document()
    {
        var testSite = new TestSite(typeof(CustomDocumentSerializer.Startup));
        var client = testSite.BuildClient();

        var swaggerResponse = await client.GetAsync($"/swagger/v1/swagger.json");

        swaggerResponse.EnsureSuccessStatusCode();
        var contentStream = await swaggerResponse.Content.ReadAsStreamAsync();
        using var document = JsonDocument.Parse(contentStream);

        // verify that the custom serializer wrote the swagger info
        var swaggerInfo = document.RootElement.GetProperty("swagger").GetString();
        Assert.Equal("DocumentSerializerTest3.0", swaggerInfo);
    }

    [Fact]
    public async Task TestSite_Writes_Custom_V2_Document()
    {
        var testSite = new TestSite(typeof(CustomDocumentSerializer.Startup));
        var client = testSite.BuildClient();

        var swaggerResponse = await client.GetAsync($"/swagger/v1/swaggerv2.json");

        swaggerResponse.EnsureSuccessStatusCode();
        var contentStream = await swaggerResponse.Content.ReadAsStreamAsync();
        using var document = JsonDocument.Parse(contentStream);

        // verify that the custom serializer wrote the swagger info
        var swaggerInfo = document.RootElement.GetProperty("swagger").GetString();
        Assert.Equal("DocumentSerializerTest2.0", swaggerInfo);
    }

    [Fact]
    public async Task DocumentProvider_Writes_Custom_V3_Document()
    {
        var testSite = new TestSite(typeof(CustomDocumentSerializer.Startup));
        var server = testSite.BuildServer();
        var services = server.Host.Services;

        var documentProvider = services.GetService<IDocumentProvider>();
        using var stream = new MemoryStream();

        using (var writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 2048, leaveOpen: true))
        {
            await documentProvider.GenerateAsync("v1", writer);
            await writer.FlushAsync();
        }

        stream.Position = 0L;

        using var document = JsonDocument.Parse(stream);

        // verify that the custom serializer wrote the swagger info
        var swaggerInfo = document.RootElement.GetProperty("swagger").GetString();
        Assert.Equal("DocumentSerializerTest3.0", swaggerInfo);
    }

    [Fact]
    public async Task DocumentProvider_Writes_Custom_V2_Document()
    {
        await DocumentProviderWritesCustomV2Document(
            (options) => options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0);
    }

    [Obsolete]
    [Fact]
    public async Task DocumentProvider_Writes_Custom_V2_Document_SerializeAsV2()
        => await DocumentProviderWritesCustomV2Document((options) => options.SerializeAsV2 = true);

    private static async Task DocumentProviderWritesCustomV2Document(Action<SwaggerOptions> configure)
    {
        var testSite = new TestSite(typeof(CustomDocumentSerializer.Startup));
        var server = testSite.BuildServer();
        var services = server.Host.Services;

        var documentProvider = services.GetService<IDocumentProvider>();
        var options = services.GetService<IOptions<SwaggerOptions>>();

        configure(options.Value);

        using var stream = new MemoryStream();

        using (var writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 2048, leaveOpen: true))
        {
            await documentProvider.GenerateAsync("v1", writer);
            await writer.FlushAsync();
        }

        stream.Position = 0L;

        using var document = JsonDocument.Parse(stream);

        // verify that the custom serializer wrote the swagger info
        var swaggerInfo = document.RootElement.GetProperty("swagger").GetString();
        Assert.Equal("DocumentSerializerTest2.0", swaggerInfo);
    }
}
