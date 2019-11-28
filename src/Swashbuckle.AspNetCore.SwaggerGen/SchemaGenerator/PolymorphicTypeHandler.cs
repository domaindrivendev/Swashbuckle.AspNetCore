using System;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class PolymorphicTypeHandler : SchemaGeneratorHandler
    {
        private readonly SchemaGeneratorOptions _generatorOptions;
        private readonly ISchemaGenerator _schemaGenerator;

        public PolymorphicTypeHandler(SchemaGeneratorOptions generatorOptions, ISchemaGenerator schemaGenerator)
        {
            _generatorOptions = generatorOptions;
            _schemaGenerator = schemaGenerator;
        }

        public override bool CanCreateSchemaFor(Type type, out bool shouldBeReferenced)
        {
            if (_generatorOptions.GeneratePolymorphicSchemas && _generatorOptions.SubTypesResolver(type).Any())
            {
                shouldBeReferenced = false;
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        public override OpenApiSchema CreateDefinitionSchema(Type type, SchemaRepository schemaRepository)
        {
            return new OpenApiSchema
            {
                OneOf = _generatorOptions.SubTypesResolver(type)
                    .Select(subType => _schemaGenerator.GenerateSchema(subType, schemaRepository))
                    .ToList()
            };
        }
    }
}