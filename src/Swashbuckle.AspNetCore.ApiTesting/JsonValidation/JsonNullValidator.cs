using Microsoft.OpenApi;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting;

public sealed class JsonNullValidator : IJsonValidator
{
    public bool CanValidate(IOpenApiSchema schema) => schema.Type == JsonSchemaTypes.Null;

    public bool Validate(
        IOpenApiSchema schema,
        OpenApiDocument openApiDocument,
        JToken instance,
        out IEnumerable<string> errorMessages)
    {
        if (instance.Type != JTokenType.Null)
        {
            errorMessages = [$"Path: {instance.Path}. Instance is not of type 'null'"];
            return false;
        }

        errorMessages = [];
        return true;
    }
}
