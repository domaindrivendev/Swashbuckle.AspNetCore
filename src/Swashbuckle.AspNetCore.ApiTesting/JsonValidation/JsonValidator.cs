using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting;

public sealed class JsonValidator : IJsonValidator
{
    private readonly IEnumerable<IJsonValidator> _subValidators;

    public JsonValidator()
    {
        _subValidators =
        [
            new JsonNullValidator(),
            new JsonBooleanValidator(),
            new JsonObjectValidator(this),
            new JsonArrayValidator(this),
            new JsonNumberValidator(),
            new JsonStringValidator(),
            new JsonAllOfValidator(this),
            new JsonAnyOfValidator(this),
            new JsonOneOfValidator(this),
        ];
    }

    public bool CanValidate(IOpenApiSchema schema) => true;

    public bool Validate(
        IOpenApiSchema schema,
        OpenApiDocument openApiDocument,
        JToken instance,
        out IEnumerable<string> errorMessages)
    {
        if (schema is OpenApiSchemaReference reference && !openApiDocument.Components.Schemas.Any((p) => p.Key == reference.Reference.Id))
        {
            throw new InvalidOperationException($"Invalid Reference identifier '{reference.Reference.Id}'.");
        }

        var errors = new List<string>();

        foreach (var subValidator in _subValidators)
        {
            if (!subValidator.CanValidate(schema))
            {
                continue;
            }

            if (!subValidator.Validate(schema, openApiDocument, instance, out IEnumerable<string> subErrorMessages))
            {
                errors.AddRange(subErrorMessages);
            }
        }

        errorMessages = errors;
        return !errorMessages.Any();
    }
}
