using System;
using System.IO;
using Microsoft.OpenApi.Models;
#if !NET10_0_OR_GREATER
using Microsoft.OpenApi.Readers;
#endif

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public static class ApiTestRunnerOptionsExtensions
    {
        public static void AddOpenApiFile(this ApiTestRunnerOptions options, string documentName, string filePath)
        {
            using var fileStream = File.OpenRead(filePath);
#if NET10_0_OR_GREATER
            using var memoryStream = new MemoryStream();

            fileStream.CopyTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            var result = OpenApiDocument.Load(memoryStream);
            options.OpenApiDocs.Add(documentName, result.Document);
#else
            var openApiDocument = new OpenApiStreamReader().Read(fileStream, out OpenApiDiagnostic diagnostic);
            options.OpenApiDocs.Add(documentName, openApiDocument);
#endif
        }

        public static OpenApiDocument GetOpenApiDocument(this ApiTestRunnerOptions options, string documentName)
        {
            if (!options.OpenApiDocs.TryGetValue(documentName, out OpenApiDocument document))
            {
                throw new InvalidOperationException($"Document with name '{documentName}' not found");
            }

            return document;
        }
    }
}
