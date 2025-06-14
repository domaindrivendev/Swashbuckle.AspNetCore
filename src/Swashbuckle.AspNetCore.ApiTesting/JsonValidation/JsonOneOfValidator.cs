using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting;

public sealed class JsonOneOfValidator(JsonValidator jsonValidator) : IJsonValidator
{
    private readonly JsonValidator _jsonValidator = jsonValidator;

    public bool CanValidate(IOpenApiSchema schema) => schema.OneOf != null && schema.OneOf.Any();

    public bool Validate(
        IOpenApiSchema schema,
        OpenApiDocument openApiDocument,
        JToken instance,
        out IEnumerable<string> errorMessages)
    {
        var errors = new List<string>();
        int matched = 0;

        if (schema.OneOf is { } oneOf)
        {
            for (int i = 0; i < oneOf.Count; i++)
            {
                if (_jsonValidator.Validate(oneOf[i], openApiDocument, instance, out IEnumerable<string> subErrorMessages))
                {
                    matched++;
                }
                else
                {
                    errors.AddRange(subErrorMessages.Select(msg => $"{msg} (oneOf[{i}])"));
                }
            }
        }

        if (matched == 0)
        {
            errorMessages = errors;
            return false;
        }

        if (matched > 1)
        {
            errorMessages = [$"Path: {instance.Path}. Instance matches multiple schemas in oneOf array"];
            return false;
        }

        errorMessages = [];
        return true;
    }
}
