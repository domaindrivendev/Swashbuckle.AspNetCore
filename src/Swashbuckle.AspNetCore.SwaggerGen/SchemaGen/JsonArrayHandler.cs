using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class JsonArrayHandler : SchemaGeneratorHandler
    {
        public JsonArrayHandler(SchemaGeneratorOptions schemaGeneratorOptions, SchemaGenerator schemaGenerator, JsonSerializerSettings jsonSerializerSettings)
            : base(schemaGeneratorOptions, schemaGenerator, jsonSerializerSettings)
        { }

        protected override bool CanGenerateSchemaFor(ModelMetadata modelMetadata, JsonContract jsonContract)
        {
            return jsonContract is JsonArrayContract;
        }

        protected override OpenApiSchema GenerateSchemaFor(ModelMetadata modelMetadata, SchemaRepository schemaRepository, JsonContract jsonContract)
        {
            var jsonArrayContract = (JsonArrayContract)jsonContract;

            var itemModelMetadata = modelMetadata.GetMetadataForType(jsonArrayContract.CollectionItemType);

            return new OpenApiSchema
            {
                Type = "array",
                Items = SchemaGenerator.GenerateSchema(itemModelMetadata, schemaRepository),
                UniqueItems = jsonArrayContract.UnderlyingType.IsSet() ? (bool?)true : null
            };
        }
    }
}