using System;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class OpenApiAnyFactory
    {
        public static bool TryCreateFor(OpenApiSchema schema, object value, out IOpenApiAny openApiAny)
        {
            if (schema.Items != null && value != null)
            {
                // value should be be an array. Allow a single value, but put it into an array for convenience
                object[] values = value.GetType().IsArray ? (object[])value : new object[] { value };
                openApiAny = new OpenApiArray();

                foreach (var v in values)
                {
                    if (TryCreateFor(schema.Items.Type, schema.Items.Format, v, out var openApiSingleValue))
                    {
                        ((OpenApiArray)openApiAny).Add(openApiSingleValue);
                    } else
                    {
                        // at least one value could not be converted, so give up and return null and false
                        openApiAny = null;
                        return false;
                    }
                }
                return ((OpenApiArray)openApiAny).Count == values.Length;
            }
            else
            {
                return TryCreateFor(schema.Type, schema.Format, value, out openApiAny);
            }
        }

        private static bool TryCreateFor(string schemaType, string schemaFormat, object value, out IOpenApiAny openApiAny)
        {
            openApiAny = null;

            if (schemaType == "boolean" && TryCast(value, out bool boolValue))
                openApiAny = new OpenApiBoolean(boolValue);

            else if (schemaType == "integer" && schemaFormat == "int32" && TryCast(value, out int intValue))
                openApiAny = new OpenApiInteger(intValue);

            else if (schemaType == "integer" && schemaFormat == "int64" && TryCast(value, out long longValue))
                openApiAny = new OpenApiLong(longValue);

            else if (schemaType == "number" && schemaFormat == "float" && TryCast(value, out float floatValue))
                openApiAny = new OpenApiFloat(floatValue);

            else if (schemaType == "number" && schemaFormat == "double" && TryCast(value, out double doubleValue))
                openApiAny = new OpenApiDouble(doubleValue);

            else if (schemaType == "string" && value.GetType().IsEnum)
                openApiAny = new OpenApiString(Enum.GetName(value.GetType(), value));

            else if (schemaType == "string" && schemaFormat == "date-time" && TryCast(value, out DateTime dateTimeValue))
                openApiAny = new OpenApiDate(dateTimeValue);

            else if (schemaType == "string")
                openApiAny = new OpenApiString(value.ToString());

            return openApiAny != null;
        }

        private static bool TryCast<T>(object value, out T typedValue)
        {
            try
            {
                typedValue = (T)Convert.ChangeType(value, typeof(T));
                return true;
            }
            catch (InvalidCastException)
            {
                typedValue = default(T);
                return false;
            }
        }
    }
}
