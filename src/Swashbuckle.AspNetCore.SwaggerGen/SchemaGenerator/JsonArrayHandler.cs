using System;
using System.Text.Json;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class JsonArrayHandler : SchemaGeneratorHandler
    {
        private readonly ISchemaGenerator _schemaGenerator;

        public JsonArrayHandler(ISchemaGenerator schemaGenerator)
        {
            _schemaGenerator = schemaGenerator;
        }

        public override bool CanCreateSchemaFor(Type type, out bool shouldBeReferenced)
        {
            if (type.IsEnumerable(out Type itemType))
            {

                shouldBeReferenced = (itemType == type); // to avoid circular reference
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        public override OpenApiSchema CreateDefinitionSchema(Type type, SchemaRepository schemaRepository)
        {
            if (!type.IsEnumerable(out Type itemType))
                throw new InvalidOperationException($"Type {type} is not enumerable");

            return new OpenApiSchema
            {
                Type = "array",
                Items = _schemaGenerator.GenerateSchema(itemType, schemaRepository),
                UniqueItems = type.IsSet() ? (bool?)true : null,
                Nullable = true
            };
        }
    }
}