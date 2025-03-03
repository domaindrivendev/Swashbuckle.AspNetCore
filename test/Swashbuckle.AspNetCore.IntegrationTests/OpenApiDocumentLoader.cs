using System.IO;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
#if NET10_0_OR_GREATER
using Microsoft.OpenApi.Reader;
#else
using Microsoft.OpenApi.Readers;
#endif

namespace Swashbuckle.AspNetCore;

internal static class OpenApiDocumentLoader
{
    public static async Task<OpenApiDocument> LoadAsync(Stream stream)
    {
#if NET10_0_OR_GREATER
        var result = await OpenApiDocument.LoadAsync(stream);
        return result.Document;
#else
        var reader = new OpenApiStreamReader();
        var document = reader.Read(stream, out OpenApiDiagnostic diagnostic);
        return await Task.FromResult(document);
#endif
    }

    public static async Task<(OpenApiDocument Document, OpenApiDiagnostic Diagnostic)> LoadWithDiagnosticsAsync(Stream stream)
    {
#if NET10_0_OR_GREATER
        var result = await OpenApiDocument.LoadAsync(stream);
        return (result.Document, result.Diagnostic);
#else
        var reader = new OpenApiStreamReader();
        var document = reader.Read(stream, out OpenApiDiagnostic diagnostic);
        return await Task.FromResult((document, diagnostic));
#endif
    }
}
