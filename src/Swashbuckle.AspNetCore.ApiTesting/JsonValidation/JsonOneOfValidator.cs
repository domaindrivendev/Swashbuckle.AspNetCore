using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public class JsonOneOfValidator(JsonValidator jsonValidator) : IJsonValidator
    {
        private readonly JsonValidator _jsonValidator = jsonValidator;

        public bool CanValidate(OpenApiSchema schema) => schema.OneOf != null && schema.OneOf.Any();

        public bool Validate(
            OpenApiSchema schema,
            OpenApiDocument openApiDocument,
            JToken instance,
            out IEnumerable<string> errorMessages)
        {
            var errorMessagesList = new List<string>();

            var oneOfArray = schema.OneOf.ToArray();

            int matched = 0;
            for (int i = 0; i < oneOfArray.Length; i++)
            {
                if (_jsonValidator.Validate(oneOfArray[i], openApiDocument, instance, out IEnumerable<string> subErrorMessages))
                {
                    matched++;
                }
                else
                {
                    errorMessagesList.AddRange(subErrorMessages.Select(msg => $"{msg} (oneOf[{i}])"));
                }
            }

            if (matched == 0)
            {
                errorMessages = errorMessagesList;
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
}
