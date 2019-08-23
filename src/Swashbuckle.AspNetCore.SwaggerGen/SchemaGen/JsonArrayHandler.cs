using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class JsonArrayHandler : SchemaGeneratorHandler
    {
        public JsonArrayHandler(SchemaGeneratorOptions schemaGeneratorOptions, ISchemaGenerator schemaGenerator)
            : base(schemaGeneratorOptions, schemaGenerator)
        { }

        protected override bool CanGenerateSchema(JsonContract jsonContract, out bool shouldBeReferenced)
        {
            if (jsonContract is JsonArrayContract jsonArrayContract)
            {
                shouldBeReferenced = (jsonArrayContract.UnderlyingType == jsonArrayContract.CollectionItemType);
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        protected override OpenApiSchema GenerateDefinitionSchema(JsonContract jsonContract, SchemaRepository schemaRepository)
        {
            var jsonArrayContract = (JsonArrayContract)jsonContract;
            var itemsType = jsonArrayContract.CollectionItemType ?? typeof(object);

            return new OpenApiSchema
            {
                Type = "array",
                Items = SchemaGenerator.GenerateSchema(itemsType, schemaRepository),
                UniqueItems = jsonArrayContract.UnderlyingType.IsSet() ? (bool?)true : null
            };
        }
    }
}