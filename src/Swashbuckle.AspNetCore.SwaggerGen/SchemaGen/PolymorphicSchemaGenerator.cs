using System;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class PolymorphicSchemaGenerator : ISchemaGenerator
    {
        private readonly SchemaGeneratorOptions _options;
        private readonly ISchemaGenerator _schemaGenerator;

        public PolymorphicSchemaGenerator(SchemaGeneratorOptions options, ISchemaGenerator schemaGenerator)
        {
            _options = options;
            _schemaGenerator = schemaGenerator;
        }

        public bool CanGenerateSchemaFor(Type type)
        {
            var subTypes = _options.SubTypesResolver(type);
            return subTypes.Any();
        }

        public OpenApiSchema GenerateSchemaFor(Type type, SchemaRepository schemaRepository)
        {
            var subTypes = _options.SubTypesResolver(type);

            return new OpenApiSchema
            {
                OneOf = subTypes
                    .Select(subType => _schemaGenerator.GenerateSchemaFor(subType, schemaRepository))
                    .ToList()
            };
        }
    }
}