using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting;

public sealed class JsonNumberValidator : IJsonValidator
{
    public bool CanValidate(IOpenApiSchema schema) => schema.Type is { } type && type.HasFlag(JsonSchemaTypes.Number);

    public bool Validate(
        IOpenApiSchema schema,
        OpenApiDocument openApiDocument,
        JToken instance,
        out IEnumerable<string> errorMessages)
    {
        if (instance.Type is not JTokenType.Float and not JTokenType.Integer)
        {
            errorMessages = [$"Path: {instance.Path}. Instance is not of type 'number'"];
            return false;
        }

        var numberValue = instance.Value<decimal>();
        var errors = new List<string>();

        // multipleOf
        if (schema.MultipleOf.HasValue && ((numberValue % schema.MultipleOf.Value) != 0))
        {
            errors.Add($"Path: {instance.Path}. Number is not evenly divisible by multipleOf");
        }

        // maximum & exclusiveMaximum
        if (schema.Maximum is { } maximum)
        {
            if (schema.ExclusiveMaximum.HasValue && (numberValue >= maximum))
            {
                errors.Add($"Path: {instance.Path}. Number is greater than, or equal to, maximum");
            }
            else if (numberValue > maximum)
            {
                errors.Add($"Path: {instance.Path}. Number is greater than maximum");
            }
        }

        // minimum & exclusiveMinimum
        if (schema.Minimum is { } minimum)
        {
            if (schema.ExclusiveMinimum.HasValue && (numberValue <= minimum))
            {
                errors.Add($"Path: {instance.Path}. Number is less than, or equal to, minimum");
            }
            else if (numberValue < minimum)
            {
                errors.Add($"Path: {instance.Path}. Number is less than minimum");
            }
        }

        errorMessages = errors;
        return !errorMessages.Any();
    }
}
