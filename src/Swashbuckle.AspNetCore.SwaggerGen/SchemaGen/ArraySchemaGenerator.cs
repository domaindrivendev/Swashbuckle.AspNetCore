using System;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class ArraySchemaGenerator : ChainableSchemaGenerator
    {
        public ArraySchemaGenerator(SchemaGeneratorOptions options, ISchemaGenerator rootGenerator, IContractResolver contractResolver)
            : base(options, rootGenerator, contractResolver)
        { }

        protected override bool CanGenerateSchemaFor(Type type)
        {
            return ContractResolver.ResolveContract(type) is JsonArrayContract;
        }

        protected override OpenApiSchema GenerateSchemaFor(Type type, SchemaRepository schemaRepository)
        {
            var jsonArrayContract = (JsonArrayContract)ContractResolver.ResolveContract(type);

            return new OpenApiSchema
            {
                Type = "array",
                Items = RootGenerator.GenerateSchema(jsonArrayContract.CollectionItemType, schemaRepository),
                UniqueItems = jsonArrayContract.UnderlyingType.IsSet() ? (bool?)true : null
            };
        }
    }
}