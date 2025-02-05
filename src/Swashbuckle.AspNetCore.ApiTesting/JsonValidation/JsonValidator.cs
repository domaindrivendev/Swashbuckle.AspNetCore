using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting
{
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

        public bool CanValidate(OpenApiSchema schema) => true;

        public bool Validate(
            OpenApiSchema schema,
            OpenApiDocument openApiDocument,
            JToken instance,
            out IEnumerable<string> errorMessages)
        {
            var errorMessagesList = new List<string>();

            schema = schema.Reference != null
                ? new OpenApiSchemaReference(schema.Reference.Id, openApiDocument)
                : schema;

            // TODO Why don't invalid references throw anymore?
            if (schema.Reference != null && !openApiDocument.Components.Schemas.Any((p) => p.Key == schema.Reference.Id))
            {
                throw new System.InvalidOperationException($"Invalid Reference identifier '{schema.Reference.Id}'.");
            }

            foreach (var subValidator in _subValidators)
            {
                if (!subValidator.CanValidate(schema))
                {
                    continue;
                }

                if (!subValidator.Validate(schema, openApiDocument, instance, out IEnumerable<string> subErrorMessages))
                {
                    errorMessagesList.AddRange(subErrorMessages);
                }
            }

            errorMessages = errorMessagesList;
            return !errorMessages.Any();
        }
    }
}
