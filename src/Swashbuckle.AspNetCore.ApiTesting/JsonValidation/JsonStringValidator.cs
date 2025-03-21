using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting;

public class JsonStringValidator : IJsonValidator
{
    public bool CanValidate(OpenApiSchema schema) => schema.Type == JsonSchemaTypes.String;

    public bool Validate(
        OpenApiSchema schema,
        OpenApiDocument openApiDocument,
        JToken instance,
        out IEnumerable<string> errorMessages)
    {
        if (instance.Type is not JTokenType.Date and not JTokenType.Guid and not JTokenType.String)
        {
            errorMessages = [$"Path: {instance.Path}. Instance is not of type 'string'"];
            return false;
        }

        var stringValue = instance.Value<string>();
        var errorMessagesList = new List<string>();

        // maxLength
        if (schema.MaxLength.HasValue && (stringValue.Length > schema.MaxLength.Value))
        {
            errorMessagesList.Add($"Path: {instance.Path}. String length is greater than maxLength");
        }

        // minLength
        if (schema.MinLength.HasValue && (stringValue.Length < schema.MinLength.Value))
        {
            errorMessagesList.Add($"Path: {instance.Path}. String length is less than minLength");
        }

        // pattern
        if ((schema.Pattern != null) && !Regex.IsMatch(stringValue, schema.Pattern))
        {
            errorMessagesList.Add($"Path: {instance.Path}. String does not match pattern");
        }

        errorMessages = errorMessagesList;
        return !errorMessages.Any();
    }
}
