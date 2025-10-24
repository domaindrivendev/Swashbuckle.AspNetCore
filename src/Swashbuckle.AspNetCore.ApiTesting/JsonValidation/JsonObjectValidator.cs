using Microsoft.OpenApi;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting;

public sealed class JsonObjectValidator(IJsonValidator jsonValidator) : IJsonValidator
{
    private readonly IJsonValidator _jsonValidator = jsonValidator;

    public bool CanValidate(IOpenApiSchema schema) => schema.Type is { } type && type.HasFlag(JsonSchemaTypes.Object);

    public bool Validate(
        IOpenApiSchema schema,
        OpenApiDocument openApiDocument,
        JToken instance,
        out IEnumerable<string> errorMessages)
    {
        if (instance.Type != JTokenType.Object)
        {
            errorMessages = [$"Path: {instance.Path}. Instance is not of type 'object'"];
            return false;
        }

        var jObject = (JObject)instance;
        var properties = jObject.Properties();
        var errors = new List<string>();

        // maxProperties
        if (schema.MaxProperties.HasValue && properties.Count() > schema.MaxProperties.Value)
        {
            errors.Add($"Path: {instance.Path}. Number of properties is greater than maxProperties");
        }

        // minProperties
        if (schema.MinProperties.HasValue && properties.Count() < schema.MinProperties.Value)
        {
            errors.Add($"Path: {instance.Path}. Number of properties is less than minProperties");
        }

        // required
        if (schema.Required != null && schema.Required.Except(properties.Select(p => p.Name)).Any())
        {
            errors.Add($"Path: {instance.Path}. Required property(s) not present");
        }

        foreach (var property in properties)
        {
            // properties
            IEnumerable<string> propertyErrorMessages;

            if (schema.Properties != null && schema.Properties.TryGetValue(property.Name, out var propertySchema))
            {
                if (!_jsonValidator.Validate(propertySchema, openApiDocument, property.Value, out propertyErrorMessages))
                {
                    errors.AddRange(propertyErrorMessages);
                }

                continue;
            }

            if (!schema.AdditionalPropertiesAllowed)
            {
                errors.Add($"Path: {instance.Path}. Additional properties not allowed");
            }

            // additionalProperties
            if (schema.AdditionalProperties != null &&
                !_jsonValidator.Validate(schema.AdditionalProperties, openApiDocument, property.Value, out propertyErrorMessages))
            {
                errors.AddRange(propertyErrorMessages);
            }
        }

        errorMessages = errors;
        return !errorMessages.Any();
    }
}
