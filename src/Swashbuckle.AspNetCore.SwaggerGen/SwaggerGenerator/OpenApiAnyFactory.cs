using System;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class OpenApiAnyFactory
    {
        public static IOpenApiAny CreateFor(OpenApiSchema schema, object value)
        {
            if (schema.Type == "boolean" && TryCast(value, out bool boolValue))
                return new OpenApiBoolean(boolValue);

            else if (schema.Type == "integer" && schema.Format == "int32" && TryCast(value, out int intValue))
                return new OpenApiInteger(intValue);

            else if (schema.Type == "integer" && schema.Format == "int64" && TryCast(value, out long longValue))
                return new OpenApiLong(longValue);

            else if (schema.Type == "number" && schema.Format == "float" && TryCast(value, out float floatValue))
                return new OpenApiFloat(floatValue);

            else if (schema.Type == "number" && schema.Format == "double" && TryCast(value, out double doubleValue))
                return new OpenApiDouble(doubleValue);

            else if (schema.Type == "string" && value.GetType().IsEnum)
                return new OpenApiString(Enum.GetName(value.GetType(), value));

            else if (schema.Type == "string" && schema.Format == "date-time" && TryCast(value, out DateTime dateTimeValue))
                return new OpenApiDate(dateTimeValue);

            else if (schema.Type == "string")
                return new OpenApiString(value.ToString());

            return null;
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
