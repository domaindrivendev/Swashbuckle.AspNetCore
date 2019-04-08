using System;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class OpenApiAnyFactory
    {
        public static bool TryCreateFor(OpenApiSchema schema, object value, out IOpenApiAny openApiAny)
        {
            openApiAny = null;

            if (schema.Type == "boolean" && TryCast(value, out bool boolValue))
                openApiAny = new OpenApiBoolean(boolValue);

            else if (schema.Type == "integer" && schema.Format == "int32" && TryCast(value, out short shortValue))
                openApiAny = new OpenApiInteger(shortValue); // preliminary unboxing is required; simply casting to int won't suffice

            else if (schema.Type == "integer" && schema.Format == "int32" && TryCast(value, out int intValue))
                openApiAny = new OpenApiInteger(intValue);

            else if (schema.Type == "integer" && schema.Format == "int64" && TryCast(value, out long longValue))
                openApiAny = new OpenApiLong(longValue);

            else if (schema.Type == "number" && schema.Format == "float" && TryCast(value, out float floatValue))
                openApiAny = new OpenApiFloat(floatValue);

            else if (schema.Type == "number" && schema.Format == "double" && TryCast(value, out double doubleValue))
                openApiAny = new OpenApiDouble(doubleValue);

            else if (schema.Type == "string" && value.GetType().IsEnum)
                openApiAny = new OpenApiString(Enum.GetName(value.GetType(), value));

            else if (schema.Type == "string" && schema.Format == "date-time" && TryCast(value, out DateTime dateTimeValue))
                openApiAny = new OpenApiDate(dateTimeValue);

            else if (schema.Type == "string")
                openApiAny = new OpenApiString(value.ToString());

            return openApiAny != null;
        }

        private static bool TryCast<T>(object value, out T typedValue)
        {
            try
            {
                typedValue = (T)value;
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
