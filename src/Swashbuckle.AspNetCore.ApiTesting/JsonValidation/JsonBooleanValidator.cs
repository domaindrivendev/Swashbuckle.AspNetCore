using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public class JsonBooleanValidator : IJsonValidator
    {
        public bool CanValidate(OpenApiSchema schema) => schema.Type == "boolean";

        public bool Validate(
            OpenApiSchema schema,
            OpenApiDocument openApiDocument,
            JToken instance,
            out IEnumerable<string> errorMessages)
        {
            if (instance.Type != JTokenType.Boolean)
            {
                errorMessages = new[] { $"Path: {instance.Path}. Instance is not of type 'boolean'" };
                return false;
            }

            errorMessages = Enumerable.Empty<string>();
            return true;
        }
    }
}