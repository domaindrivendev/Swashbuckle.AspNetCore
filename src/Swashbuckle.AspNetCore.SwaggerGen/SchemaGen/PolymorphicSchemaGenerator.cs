using System;
using System.Linq;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class PolymorphicSchemaGenerator : ChainableSchemaGenerator
    {
        public PolymorphicSchemaGenerator(
            SchemaGeneratorOptions options,
            ISchemaGenerator rootGenerator,
            IContractResolver contractResolver)
            : base(options, rootGenerator, contractResolver)
        { }

        protected override bool CanGenerateSchemaFor(Type type)
        {
            var subTypes = Options.SubTypesResolver(type);
            return subTypes.Any();
        }

        protected override OpenApiSchema GenerateSchemaFor(Type type, SchemaRepository schemaRepository)
        {
            var subTypes = Options.SubTypesResolver(type);

            return new OpenApiSchema
            {
                OneOf = subTypes
                    .Select(subType => RootGenerator.GenerateSchema(subType, schemaRepository))
                    .ToList()
            };
        }
    }
}