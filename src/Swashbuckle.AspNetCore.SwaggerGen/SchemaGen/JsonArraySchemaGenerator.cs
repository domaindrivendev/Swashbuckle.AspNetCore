using System;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class JsonArraySchemaGenerator : ISchemaGenerator
    {
        private readonly SchemaGeneratorOptions _options;
        private readonly IContractResolver _jsonContractResolver;
        private readonly ISchemaGenerator _schemaGenerator;

        public JsonArraySchemaGenerator(
            SchemaGeneratorOptions options,
            IContractResolver jsonContractResolver,
            ISchemaGenerator schemaGenerator)
        {
            _options = options;
            _jsonContractResolver = jsonContractResolver;
            _schemaGenerator = schemaGenerator;
        }

        public bool CanGenerateSchemaFor(Type type)
        {
            return _jsonContractResolver.ResolveContract(type) is JsonArrayContract;
        }

        public OpenApiSchema GenerateSchemaFor(Type type, SchemaRepository schemaRepository)
        {
            var jsonArrayContract = (JsonArrayContract)_jsonContractResolver.ResolveContract(type);

            return new OpenApiSchema
            {
                Type = "array",
                Items = _schemaGenerator.GenerateSchemaFor(jsonArrayContract.CollectionItemType, schemaRepository),
                UniqueItems = jsonArrayContract.UnderlyingType.IsSet() ? (bool?)true : null
            };
        }
    }
}