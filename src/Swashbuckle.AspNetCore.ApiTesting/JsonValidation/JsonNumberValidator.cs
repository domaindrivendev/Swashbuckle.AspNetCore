using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public class JsonNumberValidator : IJsonValidator
    {
        public bool CanValidate(OpenApiSchema schema) => schema.Type == JsonSchemaTypes.Number;

        public bool Validate(
            OpenApiSchema schema,
            OpenApiDocument openApiDocument,
            JToken instance,
            out IEnumerable<string> errorMessages)
        {
            if (instance.Type is not JTokenType.Float and not JTokenType.Integer)
            {
                errorMessages = [$"Path: {instance.Path}. Instance is not of type 'number'"];
                return false;
            }

            var numberValue = instance.Value<decimal>();
            var errorMessagesList = new List<string>();

            // multipleOf
            if (schema.MultipleOf.HasValue && ((numberValue % schema.MultipleOf.Value) != 0))
            {
                errorMessagesList.Add($"Path: {instance.Path}. Number is not evenly divisible by multipleOf");
            }

            // maximum & exclusiveMaximum
            if (schema.Maximum.HasValue)
            {
                var exclusiveMaximum = schema.ExclusiveMaximum ?? false;

                if (exclusiveMaximum && (numberValue >= schema.Maximum.Value))
                {
                    errorMessagesList.Add($"Path: {instance.Path}. Number is greater than, or equal to, maximum");
                }
                else if (numberValue > schema.Maximum.Value)
                {
                    errorMessagesList.Add($"Path: {instance.Path}. Number is greater than maximum");
                }
            }

            // minimum & exclusiveMinimum
            if (schema.Minimum.HasValue)
            {
                var exclusiveMinimum = schema.ExclusiveMinimum ?? false;

                if (exclusiveMinimum && (numberValue <= schema.Minimum.Value))
                {
                    errorMessagesList.Add($"Path: {instance.Path}. Number is less than, or equal to, minimum");
                }
                else if (numberValue < schema.Minimum.Value)
                {
                    errorMessagesList.Add($"Path: {instance.Path}. Number is less than minimum");
                }
            }

            errorMessages = errorMessagesList;
            return !errorMessages.Any();
        }
    }
}
