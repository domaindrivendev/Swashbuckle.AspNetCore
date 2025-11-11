using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.ApiTesting;

public static class OpenApiDocumentExtensions
{
    internal static bool TryFindOperationById(
        this OpenApiDocument openApiDocument,
        string operationId,
        out string pathTemplate,
        out HttpMethod operationType)
    {
        if (openApiDocument.Paths is { Count: > 0 } paths)
        {
            foreach (var pathEntry in paths)
            {
                var pathItem = pathEntry.Value;

                if (pathItem.Operations is { Count: > 0 } operations)
                {
                    foreach (var operation in operations)
                    {
                        if (operation.Value.OperationId == operationId)
                        {
                            pathTemplate = pathEntry.Key;
                            operationType = operation.Key;
                            return true;
                        }
                    }
                }
            }
        }

        pathTemplate = null;
        operationType = default;
        return false;
    }

    internal static OpenApiOperation GetOperationByPathAndType(
        this OpenApiDocument openApiDocument,
        string pathTemplate,
        HttpMethod operationType,
        out IOpenApiPathItem pathSpec)
    {
        if (openApiDocument.Paths.TryGetValue(pathTemplate, out pathSpec))
        {
            if (pathSpec.Operations.TryGetValue(operationType, out var type))
            {
                return type;
            }
        }

        throw new InvalidOperationException($"Operation with path '{pathTemplate}' and type '{operationType}' not found");
    }
}
