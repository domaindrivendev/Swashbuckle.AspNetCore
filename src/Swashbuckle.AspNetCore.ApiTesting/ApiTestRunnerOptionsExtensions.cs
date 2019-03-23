using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System;
using System.IO;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public static class ApiTestRunnerOptionsExtensions
    {
        public static void AddOpenApiFile(this ApiTestRunnerOptions options, string documentName, string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                var openApiDocument = new OpenApiStreamReader().Read(fileStream, out OpenApiDiagnostic diagnostic);
                options.OpenApiDocs.Add(documentName, openApiDocument);
            }
        }

        public static OpenApiDocument GetOpenApiDocument(this ApiTestRunnerOptions options, string documentName)
        {
            if (!options.OpenApiDocs.TryGetValue(documentName, out OpenApiDocument document))
                throw new InvalidOperationException($"Document with name '{documentName}' not found");

            return document;
        }
    }
}
