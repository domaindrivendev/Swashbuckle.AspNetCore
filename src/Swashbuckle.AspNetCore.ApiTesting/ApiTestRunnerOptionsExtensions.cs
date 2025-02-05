using System;
using System.IO;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public static class ApiTestRunnerOptionsExtensions
    {
        public static void AddOpenApiFile(this ApiTestRunnerOptions options, string documentName, string filePath)
        {
            using var fileStream = File.OpenRead(filePath);
            using var memoryStream = new MemoryStream();

            fileStream.CopyTo(memoryStream);

            var result = OpenApiDocument.Load(memoryStream);
            options.OpenApiDocs.Add(documentName, result.Document);
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
