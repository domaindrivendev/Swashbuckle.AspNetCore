using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public class JsonNullValidator : IJsonValidator
    {
        public bool CanValidate(OpenApiSchema schema) => schema.Type == "null";

        public bool Validate(
            OpenApiSchema schema,
            OpenApiDocument openApiDocument,
            JToken instance,
            out IEnumerable<string> errorMessages)
        {
            if (instance.Type != JTokenType.Null)
            {
                errorMessages = [$"Path: {instance.Path}. Instance is not of type 'null'"];
                return false;
            }

            errorMessages = [];
            return true;
        }
    }
}
