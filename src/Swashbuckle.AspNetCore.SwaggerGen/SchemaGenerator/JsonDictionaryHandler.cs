using System;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class JsonDictionaryHandler : SchemaGeneratorHandler
    {
        private readonly ISchemaGenerator _schemaGenerator;

        public JsonDictionaryHandler(ISchemaGenerator schemaGenerator)
        {
            _schemaGenerator = schemaGenerator;
        }

        public override bool CanCreateSchemaFor(Type type, out bool shouldBeReferenced)
        {
            if (type.IsDictionary(out Type keyType, out Type valueType))
            {
                shouldBeReferenced = (valueType == type); // to avoid circular reference
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        public override OpenApiSchema CreateDefinitionSchema(Type type, SchemaRepository schemaRepository)
        {
            if (!type.IsDictionary(out Type keyType, out Type valueType))
                throw new InvalidOperationException($"Type {type} is not a dictionary");

            if (keyType.IsEnum)
            {
                // This is a special case where we can include named properties based on the enum values
                return new OpenApiSchema
                {
                    Type = "object",
                    Properties = keyType.GetEnumNames()
                        .ToDictionary(
                            name => name,
                            name => _schemaGenerator.GenerateSchema(valueType, schemaRepository)
                        )
                };
            }

            return new OpenApiSchema
            {
                Type = "object",
                AdditionalPropertiesAllowed = true,
                AdditionalProperties = _schemaGenerator.GenerateSchema(valueType, schemaRepository)
            };
        }
    }
}