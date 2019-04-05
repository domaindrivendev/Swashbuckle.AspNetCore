using System.Linq;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public class JsonAnyOfValidator : IJsonValidator
    {
        private JsonValidator _jsonValidator;

        public JsonAnyOfValidator(JsonValidator jsonValidator)
        {
            _jsonValidator = jsonValidator;
        }

        public bool CanValidate(OpenApiSchema schema) => schema.AnyOf != null && schema.AnyOf.Any();

        public bool Validate(
            OpenApiSchema schema,
            OpenApiDocument openApiDocument,
            JToken instance,
            out IEnumerable<string> errorMessages)
        {
            var errorMessagesList = new List<string>();

            var anyOfArray = schema.AnyOf.ToArray();

            for (int i=0;i<anyOfArray.Length;i++)
            {
                if (_jsonValidator.Validate(anyOfArray[i], openApiDocument, instance, out IEnumerable<string> subErrorMessages))
                {
                    errorMessages = Enumerable.Empty<string>();
                    return true;
                }

                errorMessagesList.AddRange(subErrorMessages.Select(msg => $"{msg} (anyOf[{i}])"));
            }

            errorMessages = errorMessagesList;
            return !errorMessages.Any();
        }
    }
}