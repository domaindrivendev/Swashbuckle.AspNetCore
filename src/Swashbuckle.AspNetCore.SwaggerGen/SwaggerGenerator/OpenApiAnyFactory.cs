using System;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class OpenApiAnyFactory
    {
        public static IOpenApiAny CreateFor(OpenApiSchema schema, object value, SchemaRepository schemaRepository = null)
        {
            if (value == null) return null;

            var definition = (schemaRepository != null)
                ? ResolveToDefinition(schema, schemaRepository)
                : schema;

            if (definition.Type == "integer" && definition.Format == "int64" && TryCast(value, out long longValue))
                return new OpenApiLong(longValue);

            if (definition.Type == "integer" && TryCast(value, out int intValue))
                return new OpenApiInteger(intValue);

            if (definition.Type == "number" && definition.Format == "double" && TryCast(value, out double doubleValue))
                return new OpenApiDouble(doubleValue);

            if (definition.Type == "number" && TryCast(value, out float floatValue))
                return new OpenApiFloat(floatValue);

            if (definition.Type == "boolean" && TryCast(value, out bool boolValue))
                return new OpenApiBoolean(boolValue);

            if (definition.Type == "string" && definition.Format == "date" && TryCast(value, out DateTime dateValue))
                return new OpenApiDate(dateValue);

            if (definition.Type == "string" && definition.Format == "date-time" && TryCast(value, out DateTime dateTimeValue))
                return new OpenApiDate(dateTimeValue);

            if (definition.Type == "string")
                return new OpenApiString(value.ToString());

            return null;
        }

        private static OpenApiSchema ResolveToDefinition(OpenApiSchema schema, SchemaRepository schemaRepository)
        {
            if (schema.AllOf.Any())
                return ResolveToDefinition(schema.AllOf.First(), schemaRepository);

            if (schema.Reference != null && schemaRepository.Schemas.TryGetValue(schema.Reference.Id, out OpenApiSchema referencedSchema))
                return ResolveToDefinition(referencedSchema, schemaRepository);

            return schema;
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
