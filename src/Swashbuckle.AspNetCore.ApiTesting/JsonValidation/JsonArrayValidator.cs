using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public class JsonArrayValidator : IJsonValidator
    {
        private readonly IJsonValidator _jsonValidator;

        public JsonArrayValidator(IJsonValidator jsonValidator)
        {
            _jsonValidator = jsonValidator;
        }

        public bool CanValidate(OpenApiSchema schema) => schema.Type == "array";

        public bool Validate(
            OpenApiSchema schema,
            OpenApiDocument openApiDocument,
            JToken instance,
            out IEnumerable<string> errorMessages)
        {
            if (instance.Type != JTokenType.Array)
            {
                errorMessages = new[] { $"Path: {instance.Path}. Instance is not of type 'array'" };
                return false;
            }

            var arrayInstance = (JArray)instance;
            var errorMessagesList = new List<string>();

            // items
            if (schema.Items != null)
            {
                foreach (var itemInstance in arrayInstance)
                {
                    if (!_jsonValidator.Validate(schema.Items, openApiDocument, itemInstance, out IEnumerable<string> itemErrorMessages))
                        errorMessagesList.AddRange(itemErrorMessages);
                }
            }

            // maxItems
            if (schema.MaxItems.HasValue && (arrayInstance.Count() > schema.MaxItems.Value))
                errorMessagesList.Add($"Path: {instance.Path}. Array size is greater than maxItems");

            // minItems
            if (schema.MinItems.HasValue && (arrayInstance.Count() < schema.MinItems.Value))
                errorMessagesList.Add($"Path: {instance.Path}. Array size is less than minItems");

            // uniqueItems
            if (schema.UniqueItems.HasValue && (arrayInstance.Count() != arrayInstance.Distinct().Count()))
                errorMessagesList.Add($"Path: {instance.Path}. Array does not contain uniqueItems");

            errorMessages = errorMessagesList;
            return !errorMessages.Any();
        }
    }
}