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

            if (schema.Type == "boolean" && bool.TryParse(stringValue, out bool boolValue))
                typedValue = boolValue;
            else if (schema.Type == "number" && float.TryParse(stringValue, out float floatValue))
                typedValue = floatValue;
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
