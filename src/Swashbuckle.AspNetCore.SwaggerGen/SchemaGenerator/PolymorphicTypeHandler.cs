using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class PolymorphicTypeHandler : ApiModelHandler
    {
        public PolymorphicTypeHandler(SchemaGeneratorOptions options, SchemaGenerator generator)
            : base(options, generator)
        { }

        protected override bool CanGenerateSchema(ApiModel apiModel, out bool shouldBeReferenced)
        {
            if (Options.GeneratePolymorphicSchemas && Options.SubTypesResolver(apiModel.Type).Any())
            {
                shouldBeReferenced = false;
                return true;
            }

            return shouldBeReferenced = false;
        }

        protected override OpenApiSchema GenerateDefinitionSchema(ApiModel apiModel, SchemaRepository schemaRepository)
        {
            var schema = new OpenApiSchema
            {
                OneOf = new List<OpenApiSchema>()
            };

            foreach (var subType in Options.SubTypesResolver(apiModel.Type))
            {
                schema.OneOf.Add(Generator.GenerateSchema(subType, schemaRepository));
            }

            return schema;
        }
    }
}