using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class JsonDictionaryHandler : SchemaGeneratorHandler
    {
        public JsonDictionaryHandler(SchemaGeneratorOptions schemaGeneratorOptions, SchemaGenerator schemaGenerator, JsonSerializerSettings jsonSerializerSettings)
            : base(schemaGeneratorOptions, schemaGenerator, jsonSerializerSettings)
        { }

        protected override bool CanGenerateSchemaFor(ModelMetadata modelMetadata, JsonContract jsonContract)
        {
            return jsonContract is JsonDictionaryContract;
        }

        protected override OpenApiSchema GenerateSchemaFor(ModelMetadata modelMetadata, SchemaRepository schemaRepository, JsonContract jsonContract)
        {
            var jsonDictionaryContract = (JsonDictionaryContract)jsonContract;

            var keyType = jsonDictionaryContract.DictionaryKeyType ?? typeof(object);
            var valueType = jsonDictionaryContract.DictionaryValueType ?? typeof(object);

            if (keyType.IsEnum)
            {
                // This is a special case where we can include named properties based on the enum values
                return new OpenApiSchema
                {
                    Type = "object",
                    Properties = jsonDictionaryContract.DictionaryKeyType.GetEnumNames()
                        .ToDictionary(
                            name => name,
                            name => SchemaGenerator.GenerateSchema(modelMetadata.GetMetadataForType(valueType), schemaRepository)
                        )
                };
            }

            return new OpenApiSchema
            {
                Type = "object",
                AdditionalPropertiesAllowed = true,
                AdditionalProperties = SchemaGenerator.GenerateSchema(modelMetadata.GetMetadataForType(valueType), schemaRepository)
            };
        }
    }
}