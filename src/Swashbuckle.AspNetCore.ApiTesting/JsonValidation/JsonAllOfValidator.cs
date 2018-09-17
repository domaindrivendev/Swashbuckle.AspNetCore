using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public class JsonAllOfValidator : IJsonValidator
    {
        private JsonValidator _jsonValidator;

        public JsonAllOfValidator(JsonValidator jsonValidator)
        {
            _jsonValidator = jsonValidator;
        }

        public bool CanValidate(OpenApiSchema schema) => schema.AllOf != null && schema.AllOf.Any();

        public bool Validate(
            OpenApiSchema schema,
            OpenApiDocument openApiDocument,
            JToken instance,
            out IEnumerable<string> errorMessages)
        {
            var errorMessagesList = new List<string>();

            var allOfArray = schema.AllOf.ToArray();

            for (int i=0;i<allOfArray.Length;i++)
            {
                if (!_jsonValidator.Validate(allOfArray[i], openApiDocument, instance, out IEnumerable<string> subErrorMessages))
                    errorMessagesList.AddRange(subErrorMessages.Select(msg => $"{msg} (allOf[{i}])"));
            }

            errorMessages = errorMessagesList;
            return !errorMessages.Any();
        }
    }
}