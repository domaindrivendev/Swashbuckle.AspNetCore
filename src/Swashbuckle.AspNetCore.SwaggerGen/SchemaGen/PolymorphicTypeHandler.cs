using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class PolymorphicTypeHandler : SchemaGeneratorHandler
    {
        public PolymorphicTypeHandler(SchemaGeneratorOptions schemaGeneratorOptions, SchemaGenerator schemaGenerator)
            : base(schemaGeneratorOptions, schemaGenerator)
        { }

        protected override bool CanGenerateSchema(JsonContract jsonContract, out bool shouldBeReferenced)
        {
            if (SchemaGeneratorOptions.GeneratePolymorphicSchemas
                && SchemaGeneratorOptions.SubTypesResolver(jsonContract.UnderlyingType).Any())
            {
                shouldBeReferenced = false;
                return true;
            }

            return shouldBeReferenced = false;
        }

        protected override OpenApiSchema GenerateDefinitionSchema(JsonContract jsonContract, SchemaRepository schemaRepository)
        {
            var schema = new OpenApiSchema
            {
                OneOf = new List<OpenApiSchema>()
            };

            foreach (var subType in SchemaGeneratorOptions.SubTypesResolver(jsonContract.UnderlyingType))
            {
                schema.OneOf.Add(SchemaGenerator.GenerateSchema(subType, schemaRepository));
            }

            return schema;
        }
    }
}