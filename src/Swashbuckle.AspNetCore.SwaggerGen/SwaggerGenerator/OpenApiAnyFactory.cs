using System;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class OpenApiAnyFactory
    {
        public static IOpenApiAny CreateFor(OpenApiSchema schema, object value) => CreateFor(schema, null, null, value);

        public static IOpenApiAny CreateFor(OpenApiSchema schema, SchemaRepository schemaRepository, DataContract dataContract, object value)
        {
            if (value == null) return null;

            OpenApiSchema replacementSchema = null;
            if (schema.AllOf.Count > 0 && schemaRepository != null)
            {
                // will set null in self-referencing loop
                schemaRepository.Schemas.TryGetValue(schema.AllOf[0].Reference.Id, out replacementSchema);
            }

            var typeSchema = replacementSchema ?? schema;

            if (typeSchema.Type == "integer" && typeSchema.Format == "int64" && TryCast(value, out long longValue))
                return new OpenApiLong(longValue);

            else if (typeSchema.Type == "integer" && TryCast(value, out int intValue))
                return new OpenApiInteger(intValue);

            else if (typeSchema.Type == "number" && typeSchema.Format == "double" && TryCast(value, out double doubleValue))
                return new OpenApiDouble(doubleValue);

            else if (typeSchema.Type == "number" && TryCast(value, out float floatValue))
                return new OpenApiFloat(floatValue);

            if (typeSchema.Type == "boolean" && TryCast(value, out bool boolValue))
                return new OpenApiBoolean(boolValue);

            else if (typeSchema.Type == "string" && typeSchema.Format == "date" && TryCast(value, out DateTime dateValue))
                return new OpenApiDate(dateValue);

            else if (typeSchema.Type == "string" && typeSchema.Format == "date-time" && TryCast(value, out DateTime dateTimeValue))
                return new OpenApiDate(dateTimeValue);

            else if (typeSchema.Type == "string" && value.GetType().IsEnum)
            {
                if (dataContract != null && dataContract.EnumValues != null)
                    return new OpenApiString(dataContract.EnumValues[value].ToString());
                else
                    return new OpenApiString(Enum.GetName(value.GetType(), value));
            }

            else if (typeSchema.Type == "string")
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
