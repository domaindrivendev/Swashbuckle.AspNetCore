using System.Text;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.ApiTesting;

internal static class OpenApiSchemaExtensions
{
    internal static bool TryParse(this OpenApiSchema schema, string stringValue, out object typedValue)
    {
        typedValue = null;

        if (IsType(schema.Type, JsonSchemaTypes.Integer) && schema.Format == "int64" && long.TryParse(stringValue, out long longValue))
        {
            typedValue = longValue;
        }
        else if (IsType(schema.Type, JsonSchemaTypes.Integer) && int.TryParse(stringValue, out int intValue))
        {
            typedValue = intValue;
        }
        else if (IsType(schema.Type, JsonSchemaTypes.Number) && schema.Format == "double" && double.TryParse(stringValue, out double doubleValue))
        {
            typedValue = doubleValue;
        }
        else if (IsType(schema.Type, JsonSchemaTypes.Number) && float.TryParse(stringValue, out float floatValue))
        {
            typedValue = floatValue;
        }
        else if (IsType(schema.Type, JsonSchemaTypes.String) && schema.Format == "byte" && byte.TryParse(stringValue, out byte byteValue))
        {
            typedValue = byteValue;
        }
        else if (IsType(schema.Type, JsonSchemaTypes.Boolean) && bool.TryParse(stringValue, out bool boolValue))
        {
            typedValue = boolValue;
        }
        else if (IsType(schema.Type, JsonSchemaTypes.String) && schema.Format == "date" && DateTime.TryParse(stringValue, out DateTime dateValue))
        {
            typedValue = dateValue;
        }
        else if (IsType(schema.Type, JsonSchemaTypes.String) && schema.Format == "date-time" && DateTime.TryParse(stringValue, out DateTime dateTimeValue))
        {
            typedValue = dateTimeValue;
        }
        else if (IsType(schema.Type, JsonSchemaTypes.String) && schema.Format == "uuid" && Guid.TryParse(stringValue, out Guid uuidValue))
        {
            typedValue = uuidValue;
        }
        else if (IsType(schema.Type, JsonSchemaTypes.String))
        {
            typedValue = stringValue;
        }
        else if (IsType(schema.Type, JsonSchemaTypes.Array))
        {
            var arrayValue = schema.Items == null
                ? stringValue.Split(',')
                : stringValue.Split(',').Select(itemStringValue =>
                {
                    object itemTypedValue = null;
                    if (schema.Items is OpenApiSchema items)
                    {
                        _ = items.TryParse(itemStringValue, out itemTypedValue);
                    }
                    return itemTypedValue;
                });

            typedValue = !arrayValue.Any(itemTypedValue => itemTypedValue == null) ? arrayValue : null;
        }

        return typedValue != null;

        static bool IsType(JsonSchemaType? type, JsonSchemaType target)
            => type is { } value && value.HasFlag(target);
    }

    internal static string TypeIdentifier(this OpenApiSchema schema)
    {
        var idBuilder = new StringBuilder();

        idBuilder.Append(schema.Type.ToString().ToLowerInvariant());

        if (schema.Type == JsonSchemaTypes.Array && schema.Items != null)
        {
            idBuilder.Append($"[{schema.Items.Type}]");
        }

        return idBuilder.ToString();
    }
}
