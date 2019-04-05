using System.Linq;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public class JsonValidator : IJsonValidator
    {
        private readonly IEnumerable<IJsonValidator> _subValidators;

        public JsonValidator()
        {
            _subValidators = new IJsonValidator[]
            {
                new JsonNullValidator(),
                new JsonBooleanValidator(),
                new JsonObjectValidator(this),
                new JsonArrayValidator(this),
                new JsonNumberValidator(),
                new JsonStringValidator(),
                new JsonAllOfValidator(this),
                new JsonAnyOfValidator(this),
                new JsonOneOfValidator(this),
            };
        }

        public bool CanValidate(OpenApiSchema schema) => true;

        public bool Validate(
            OpenApiSchema schema,
            OpenApiDocument openApiDocument,
            JToken instance,
            out IEnumerable<string> errorMessages)
        {
            schema = (schema.Reference != null)
                ? (OpenApiSchema)openApiDocument.ResolveReference(schema.Reference)
                : schema;

            var errorMessagesList = new List<string>();

            foreach (var subValidator in _subValidators)
            {
                if (!subValidator.CanValidate(schema)) continue;

                if (!subValidator.Validate(schema, openApiDocument, instance, out IEnumerable<string> subErrorMessages))
                    errorMessagesList.AddRange(subErrorMessages);
            }

            errorMessages = errorMessagesList;
            return !errorMessages.Any();
        }
    }
}