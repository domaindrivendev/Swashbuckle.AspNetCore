using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public sealed class JsonBooleanValidator : IJsonValidator
    {
        public bool CanValidate(OpenApiSchema schema) => schema.Type is JsonSchemaType.Boolean;

        public bool Validate(
            OpenApiSchema schema,
            OpenApiDocument openApiDocument,
            JToken instance,
            out IEnumerable<string> errorMessages)
        {
            if (instance.Type != JTokenType.Boolean)
            {
                errorMessages = [$"Path: {instance.Path}. Instance is not of type 'boolean'"];
                return false;
            }

            errorMessages = [];
            return true;
        }
    }
}
