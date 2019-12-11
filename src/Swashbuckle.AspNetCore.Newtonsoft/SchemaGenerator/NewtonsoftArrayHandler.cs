using System;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Newtonsoft
{
    public class NewtonsoftArrayHandler : SchemaGeneratorHandler
    {
        private readonly IContractResolver _contractResolver;
        private readonly ISchemaGenerator _schemaGenerator;

        public NewtonsoftArrayHandler(IContractResolver contractResolver, ISchemaGenerator schemaGenerator)
        {
            _contractResolver = contractResolver;
            _schemaGenerator = schemaGenerator;
        }

        public override bool CanCreateSchemaFor(Type type, out bool shouldBeReferenced)
        {
            if (_contractResolver.ResolveContract(type) is JsonArrayContract jsonArrayContract)
            {
                shouldBeReferenced = (jsonArrayContract.CollectionItemType == type); // to avoid circular references
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        public override OpenApiSchema CreateDefinitionSchema(Type type, SchemaRepository schemaRepository)
        {
            if (!(_contractResolver.ResolveContract(type) is JsonArrayContract jsonArrayContract))
                throw new InvalidOperationException($"Type {type} does not resolve to a JsonArrayContract");

            var itemType = jsonArrayContract.CollectionItemType ?? typeof(object);

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