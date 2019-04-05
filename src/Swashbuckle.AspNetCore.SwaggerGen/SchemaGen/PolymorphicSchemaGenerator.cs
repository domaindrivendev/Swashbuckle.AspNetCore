using System;
using System.Linq;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class PolymorphicSchemaGenerator : ChainableSchemaGenerator
    {
        public PolymorphicSchemaGenerator(
            IContractResolver contractResolver,
            ISchemaGenerator rootGenerator,
            SchemaGeneratorOptions options)
            : base(contractResolver, rootGenerator, options)
        { }

        protected override bool CanGenerateSchemaFor(Type type)
        {
            return (Options.GeneratePolymorphicSchemas && Options.SubTypesResolver(type).Any());
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