using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.ApiDescriptions;

internal class DocumentProvider(
    IOptions<SwaggerGeneratorOptions> generatorOptions,
    IOptions<SwaggerOptions> options,
    IAsyncSwaggerProvider swaggerProvider) : IDocumentProvider
{
    private readonly SwaggerGeneratorOptions _generatorOptions = generatorOptions.Value;
    private readonly SwaggerOptions _options = options.Value;
    private readonly IAsyncSwaggerProvider _swaggerProvider = swaggerProvider;

    public IEnumerable<string> GetDocumentNames()
        => _generatorOptions.SwaggerDocs.Keys;

    public async Task GenerateAsync(string documentName, TextWriter writer)
    {
        // Let UnknownSwaggerDocument or other exception bubble up to caller.
        var swagger = await _swaggerProvider.GetSwaggerAsync(documentName, host: null, basePath: null);
        var jsonWriter = new OpenApiJsonWriter(writer);

        if (_options.CustomDocumentSerializer != null)
        {
            _options.CustomDocumentSerializer.SerializeDocument(swagger, jsonWriter, _options.OpenApiVersion);
        }
        else
        {
            swagger.SerializeAs(_options.OpenApiVersion, jsonWriter);
        }
    }
}
