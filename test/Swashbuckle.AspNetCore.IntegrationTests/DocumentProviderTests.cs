using System.Text;
using Microsoft.Extensions.ApiDescriptions;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.IntegrationTests;

[Collection("TestSite")]
public class DocumentProviderTests
{
    [Theory]
    [InlineData(typeof(Basic.Startup), new[] { "v1" })]
    [InlineData(typeof(CustomUIConfig.Startup), new[] { "v1" })]
    [InlineData(typeof(CustomUIIndex.Startup), new[] { "v1" })]
    [InlineData(typeof(GenericControllers.Startup), new[] { "v1" })]
    [InlineData(typeof(MultipleVersions.Startup), new[] { "1.0", "2.0" })]
    [InlineData(typeof(OAuth2Integration.Startup), new[] { "v1" })]
    public void DocumentProvider_ExposesAllDocumentNames(Type startupType, string[] expectedNames)
    {
        var testSite = new TestSite(startupType);
        var server = testSite.BuildServer();
        var services = server.Host.Services;
        var documentProvider = (IDocumentProvider)services.GetService(typeof(IDocumentProvider));

        var documentNames = documentProvider.GetDocumentNames();

        Assert.Equal(expectedNames, documentNames);
    }

    [Theory]
    [InlineData(typeof(Basic.Startup), "v1")]
    [InlineData(typeof(CustomUIConfig.Startup), "v1")]
    [InlineData(typeof(CustomUIIndex.Startup), "v1")]
    [InlineData(typeof(GenericControllers.Startup), "v1")]
    [InlineData(typeof(MultipleVersions.Startup), "2.0")]
    [InlineData(typeof(OAuth2Integration.Startup), "v1")]
    public async Task DocumentProvider_ExposesGeneratedSwagger(Type startupType, string documentName)
    {
        var testSite = new TestSite(startupType);
        var server = testSite.BuildServer();
        var services = server.Host.Services;

        var documentProvider = (IDocumentProvider)services.GetService(typeof(IDocumentProvider));
        using var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 2048, leaveOpen: true))
        {
            await documentProvider.GenerateAsync(documentName, writer);
            await writer.FlushAsync();
        }

        stream.Position = 0L;
        var (_, diagnostic) = await OpenApiDocumentLoader.LoadWithDiagnosticsAsync(stream);
        Assert.NotNull(diagnostic);
        Assert.Empty(diagnostic.Errors);
    }

    [Fact]
    public async Task DocumentProvider_ThrowsUnknownDocument_IfUnknownDocumentName()
    {
        var testSite = new TestSite(typeof(Basic.Startup));
        var server = testSite.BuildServer();
        var services = server.Host.Services;

        var documentProvider = (IDocumentProvider)services.GetService(typeof(IDocumentProvider));
        using var writer = new StringWriter();
        await Assert.ThrowsAsync<UnknownSwaggerDocument>(
            () => documentProvider.GenerateAsync("NotADocument", writer));
    }
}
