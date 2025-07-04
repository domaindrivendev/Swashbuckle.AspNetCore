using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace Swashbuckle.AspNetCore;

internal static class OpenApiDocumentLoader
{
    public static async Task<OpenApiDocument> LoadAsync(Stream stream)
    {
        var result = await OpenApiDocument.LoadAsync(stream);
        return result.Document;
    }

    public static async Task<(OpenApiDocument Document, OpenApiDiagnostic Diagnostic)> LoadWithDiagnosticsAsync(Stream stream)
    {
        var settings = new OpenApiReaderSettings();
        settings.AddYamlReader();

        var result = await OpenApiDocument.LoadAsync(stream, settings: settings);
        return (result.Document, result.Diagnostic);
    }
}
