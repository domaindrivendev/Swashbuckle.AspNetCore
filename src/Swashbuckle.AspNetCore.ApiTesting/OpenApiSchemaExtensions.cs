using System;
using System.Linq;
using System.Text;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public static class OpenApiSchemaExtensions
    {
        internal static bool TryParse(this OpenApiSchema schema, string stringValue, out object typedValue)
        {
            typedValue = null;

            if (schema.Type == "integer" && schema.Format == "int64" && long.TryParse(stringValue, out long longValue))
                typedValue = longValue;

            else if (schema.Type == "integer" && int.TryParse(stringValue, out int intValue))
                typedValue = intValue;

            else if (schema.Type == "number" && schema.Format == "double" && double.TryParse(stringValue, out double doubleValue))
                typedValue = doubleValue;

            else if (schema.Type == "number" && float.TryParse(stringValue, out float floatValue))
                typedValue = floatValue;

            else if (schema.Type == "string" && schema.Format == "byte" && byte.TryParse(stringValue, out byte byteValue))
                typedValue = byteValue;

            else if (schema.Type == "boolean" && bool.TryParse(stringValue, out bool boolValue))
                typedValue = boolValue;

            else if (schema.Type == "string" && schema.Format == "date" && DateTime.TryParse(stringValue, out DateTime dateValue))
                typedValue = dateValue;

            else if (schema.Type == "string" && schema.Format == "date-time" && DateTime.TryParse(stringValue, out DateTime dateTimeValue))
                typedValue = dateTimeValue;

            else if (schema.Type == "string" && schema.Format == "uuid" && Guid.TryParse(stringValue, out Guid uuidValue))
                typedValue = uuidValue;

            else if (schema.Type == "string")
                typedValue = stringValue;

            else if (schema.Type == "array")
            {
                var arrayValue = (schema.Items == null)
                    ? stringValue.Split(',')
                    : stringValue.Split(',').Select(itemStringValue =>
                    {
                        schema.Items.TryParse(itemStringValue, out object itemTypedValue);
                        return itemTypedValue;
                    });

                typedValue = !arrayValue.Any(itemTypedValue => itemTypedValue == null) ? arrayValue : null;
            }

            return typedValue != null;
        }

        internal static string TypeIdentifier(this OpenApiSchema schema)
        {
            var idBuilder = new StringBuilder(schema.Type);

            if (schema.Type == "array" && schema.Items != null)
                idBuilder.Append($"[{schema.Items.Type}]");

            return idBuilder.ToString();
        }
    }
}
