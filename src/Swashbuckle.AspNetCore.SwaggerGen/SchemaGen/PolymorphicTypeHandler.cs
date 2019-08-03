using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class PolymorphicTypeHandler : SchemaGeneratorHandler
    {
        public PolymorphicTypeHandler(SchemaGeneratorOptions schemaGeneratorOptions, ISchemaGenerator schemaGenerator, JsonSerializerSettings jsonSerializerSettings)
            : base(schemaGeneratorOptions, schemaGenerator, jsonSerializerSettings)
        { }

        protected override bool CanGenerateSchemaFor(ModelMetadata modelMetadata, JsonContract jsonContract)
        {
            return SchemaGeneratorOptions.SubTypesResolver(modelMetadata.ModelType).Any();
        }

        protected override OpenApiSchema GenerateSchemaFor(ModelMetadata modelMetadata, SchemaRepository schemaRepository, JsonContract jsonContract)
        {
            var schema = new OpenApiSchema
            {
                OneOf = new List<OpenApiSchema>()
            };

            foreach (var subType in SchemaGeneratorOptions.SubTypesResolver(modelMetadata.ModelType))
            {
                var subTypeMetadata = modelMetadata.GetMetadataForType(subType);
                var subSchema = SchemaGenerator.GenerateSchema(subTypeMetadata, schemaRepository);

                schema.OneOf.Add(subSchema);
            }

            return schema;
        }
    }
}