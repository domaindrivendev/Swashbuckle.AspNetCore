using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting;

public sealed class JsonAllOfValidator(JsonValidator jsonValidator) : IJsonValidator
{
    private readonly JsonValidator _jsonValidator = jsonValidator;

    public bool CanValidate(IOpenApiSchema schema) => schema.AllOf != null && schema.AllOf.Any();

    public bool Validate(
        IOpenApiSchema schema,
        OpenApiDocument openApiDocument,
        JToken instance,
        out IEnumerable<string> errorMessages)
    {
        var errors = new List<string>();

        if (schema.AllOf is { } allOf)
        {
            for (int i = 0; i < allOf.Count; i++)
            {
                if (!_jsonValidator.Validate(allOf[i], openApiDocument, instance, out IEnumerable<string> subErrorMessages))
                {
                    errors.AddRange(subErrorMessages.Select(msg => $"{msg} (allOf[{i}])"));
                }
            }
        }

        errorMessages = errors;
        return !errorMessages.Any();
    }
}
