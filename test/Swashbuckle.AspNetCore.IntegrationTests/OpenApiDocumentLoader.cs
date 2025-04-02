using Microsoft.OpenApi.Models;
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
        var result = await OpenApiDocument.LoadAsync(stream);
        return (result.Document, result.Diagnostic);
    }
}
