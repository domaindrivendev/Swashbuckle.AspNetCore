using Microsoft.OpenApi;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting;

public sealed class JsonAnyOfValidator(JsonValidator jsonValidator) : IJsonValidator
{
    private readonly JsonValidator _jsonValidator = jsonValidator;

    public bool CanValidate(IOpenApiSchema schema) => schema.AnyOf != null && schema.AnyOf.Any();

    public bool Validate(
        IOpenApiSchema schema,
        OpenApiDocument openApiDocument,
        JToken instance,
        out IEnumerable<string> errorMessages)
    {
        var errors = new List<string>();

        if (schema.AnyOf is { } anyOf)
        {
            for (int i = 0; i < anyOf.Count; i++)
            {
                if (_jsonValidator.Validate(anyOf[i], openApiDocument, instance, out IEnumerable<string> subErrorMessages))
                {
                    errorMessages = [];
                    return true;
                }

                errors.AddRange(subErrorMessages.Select(msg => $"{msg} (anyOf[{i}])"));
            }
        }

        errorMessages = errors;
        return !errorMessages.Any();
    }
}
