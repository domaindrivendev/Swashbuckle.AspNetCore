using System.Linq;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class JsonDictionaryHandler : SchemaGeneratorHandler
    {
        public JsonDictionaryHandler(SchemaGeneratorOptions schemaGeneratorOptions, ISchemaGenerator schemaGenerator)
            : base(schemaGeneratorOptions, schemaGenerator)
        { }

        protected override bool CanGenerateSchema(JsonContract jsonContract, out bool shouldBeReferenced)
        {
            if (jsonContract is JsonDictionaryContract jsonDictionaryContract)
            {
                shouldBeReferenced = (jsonDictionaryContract.UnderlyingType == jsonDictionaryContract.DictionaryValueType);
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        protected override OpenApiSchema GenerateDefinitionSchema(JsonContract jsonContract, SchemaRepository schemaRepository)
        {
            var jsonDictionaryContract = (JsonDictionaryContract)jsonContract;
            var keysType = jsonDictionaryContract.DictionaryKeyType ?? typeof(object);
            var valuesType = jsonDictionaryContract.DictionaryValueType ?? typeof(object);

            if (keysType.IsEnum)
            {
                // This is a special case where we can include named properties based on the enum values
                return new OpenApiSchema
                {
                    Type = "object",
                    Properties = keysType.GetEnumNames()
                        .ToDictionary(
                            name => name,
                            name => SchemaGenerator.GenerateSchema(valuesType, schemaRepository)
                        )
                };
            }

            return new OpenApiSchema
            {
                Type = "object",
                AdditionalPropertiesAllowed = true,
                AdditionalProperties = SchemaGenerator.GenerateSchema(valuesType, schemaRepository)
            };
        }
    }
}