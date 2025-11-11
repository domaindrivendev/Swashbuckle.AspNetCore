using Microsoft.OpenApi;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting;

public sealed class JsonArrayValidator(IJsonValidator jsonValidator) : IJsonValidator
{
    private readonly IJsonValidator _jsonValidator = jsonValidator;

    public bool CanValidate(IOpenApiSchema schema) => schema.Type is { } type && type.HasFlag(JsonSchemaTypes.Array);

    public bool Validate(
        IOpenApiSchema schema,
        OpenApiDocument openApiDocument,
        JToken instance,
        out IEnumerable<string> errorMessages)
    {
        if (instance.Type != JTokenType.Array)
        {
            errorMessages = [$"Path: {instance.Path}. Instance is not of type 'array'"];
            return false;
        }

        var arrayInstance = (JArray)instance;
        var errors = new List<string>();

        // items
        if (schema.Items != null)
        {
            foreach (var itemInstance in arrayInstance)
            {
                if (!_jsonValidator.Validate(schema.Items, openApiDocument, itemInstance, out IEnumerable<string> itemErrorMessages))
                {
                    errors.AddRange(itemErrorMessages);
                }
            }
        }

        // maxItems
        if (schema.MaxItems.HasValue && (arrayInstance.Count > schema.MaxItems.Value))
        {
            errors.Add($"Path: {instance.Path}. Array size is greater than maxItems");
        }

        // minItems
        if (schema.MinItems.HasValue && (arrayInstance.Count < schema.MinItems.Value))
        {
            errors.Add($"Path: {instance.Path}. Array size is less than minItems");
        }

        // uniqueItems
        if (schema.UniqueItems.HasValue && (arrayInstance.Count != arrayInstance.Distinct().Count()))
        {
            errors.Add($"Path: {instance.Path}. Array does not contain uniqueItems");
        }

        errorMessages = errors;
        return !errorMessages.Any();
    }
}
