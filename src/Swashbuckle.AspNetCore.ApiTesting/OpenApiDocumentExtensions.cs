using System;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public static class OpenApiDocumentExtensions
    {
        internal static bool TryFindOperationById(
            this OpenApiDocument openApiDocument,
            string operationId,
            out string pathTemplate,
            out OperationType operationType)
        {
            foreach (var pathEntry in openApiDocument.Paths ?? new OpenApiPaths())
            {
                var pathItem = pathEntry.Value;

                foreach (var operationEntry in pathItem.Operations)
                {
                    if (operationEntry.Value.OperationId == operationId)
                    {
                        pathTemplate = pathEntry.Key;
                        operationType = operationEntry.Key;
                        return true;
                    }
                }
            }

            pathTemplate = null;
            operationType = default(OperationType);
            return false;
        }

        internal static OpenApiOperation GetOperationByPathAndType(
            this OpenApiDocument openApiDocument,
            string pathTemplate,
            OperationType operationType,
            out OpenApiPathItem pathSpec)
        {
            if (openApiDocument.Paths.TryGetValue(pathTemplate, out pathSpec))
            {
                if (pathSpec.Operations.ContainsKey(operationType))
                    return pathSpec.Operations[operationType];
            }

            throw new InvalidOperationException($"Operation with path '{pathTemplate}' and type '{operationType}' not found");
        }
    }
}
