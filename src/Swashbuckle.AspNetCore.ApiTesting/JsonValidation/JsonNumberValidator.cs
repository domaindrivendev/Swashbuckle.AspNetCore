using System.Globalization;
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
        if (schema.MultipleOf is { } multipleOf && (numberValue % multipleOf) != 0)
        {
            errors.Add($"Path: {instance.Path}. Number is not evenly divisible by multipleOf");
        }

        if (schema.ExclusiveMaximum is { } exclusiveMaximum &&
            decimal.TryParse(exclusiveMaximum, out var exclusiveMaximumValue) &&
            numberValue >= exclusiveMaximumValue)
        {
            errors.Add($"Path: {instance.Path}. Number is greater than, or equal to, maximum");
        }

        if (schema.Maximum is { } maximum &&
            decimal.TryParse(maximum, out var maximumValue) &&
            numberValue > maximumValue)
        {
            errors.Add($"Path: {instance.Path}. Number is greater than maximum");
        }

        if (schema.ExclusiveMinimum is { } exclusiveMinimum &&
            decimal.TryParse(exclusiveMinimum, out var exclusiveMinimumValue) &&
            numberValue <= exclusiveMinimumValue)
        {
            errors.Add($"Path: {instance.Path}. Number is less than, or equal to, minimum");
        }

        if (schema.Minimum is { } minimum &&
            decimal.TryParse(minimum, out var minimumValue) &&
            numberValue < minimumValue)
        {
            errors.Add($"Path: {instance.Path}. Number is less than minimum");
        }

        errorMessages = errors;
        return !errorMessages.Any();
    }
}
