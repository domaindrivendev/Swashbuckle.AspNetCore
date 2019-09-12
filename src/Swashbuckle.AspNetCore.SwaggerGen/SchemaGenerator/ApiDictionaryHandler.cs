using System.Linq;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class ApiDictionaryHandler : ApiModelHandler
    {
        public ApiDictionaryHandler(SchemaGeneratorOptions options, ISchemaGenerator schemaGenerator)
            : base(options, schemaGenerator)
        { }

        protected override bool CanGenerateSchema(ApiModel apiModel, out bool shouldBeReferenced)
        {
            if (apiModel is ApiDictionary apiDictionary)
            {
                shouldBeReferenced = (apiDictionary.Type == apiDictionary.DictionaryValueType);
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        protected override OpenApiSchema GenerateDefinitionSchema(ApiModel apiModel, SchemaRepository schemaRepository)
        {
            var apiDictionary = (ApiDictionary)apiModel;

            if (apiDictionary.DictionaryKeyType.IsEnum)
            {
                // This is a special case where we can include named properties based on the enum values
                return new OpenApiSchema
                {
                    Type = "object",
                    Properties = apiDictionary.DictionaryKeyType.GetEnumNames()
                        .ToDictionary(
                            name => name,
                            name => Generator.GenerateSchema(apiDictionary.DictionaryValueType, schemaRepository)
                        )
                };
            }

            return new OpenApiSchema
            {
                Type = "object",
                AdditionalPropertiesAllowed = true,
                AdditionalProperties = Generator.GenerateSchema(apiDictionary.DictionaryValueType, schemaRepository)
            };
        }
    }
}