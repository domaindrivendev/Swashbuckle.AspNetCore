using System.IO;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Swashbuckle.AspNetCore;

internal static class OpenApiDocumentLoader
{
    public static async Task<OpenApiDocument> LoadAsync(Stream stream)
    {
        var reader = new OpenApiStreamReader();
        var document = reader.Read(stream, out OpenApiDiagnostic diagnostic);
        return await Task.FromResult(document);
    }

    public static async Task<(OpenApiDocument Document, OpenApiDiagnostic Diagnostic)> LoadWithDiagnosticsAsync(Stream stream)
    {
        var reader = new OpenApiStreamReader();
        var document = reader.Read(stream, out OpenApiDiagnostic diagnostic);
        return await Task.FromResult((document, diagnostic));
    }
}
