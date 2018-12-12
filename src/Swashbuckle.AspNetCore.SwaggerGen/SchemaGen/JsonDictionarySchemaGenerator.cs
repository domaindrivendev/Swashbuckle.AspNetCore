using System;
using System.Linq;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class JsonDictionarySchemaGenerator : ISchemaGenerator
    {
        private readonly SchemaGeneratorOptions _options;
        private readonly IContractResolver _jsonContractResolver;
        private readonly ISchemaGenerator _schemaGenerator;

        public JsonDictionarySchemaGenerator(
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
            return _jsonContractResolver.ResolveContract(type) is JsonDictionaryContract;
        }

        public OpenApiSchema GenerateSchemaFor(Type type, SchemaRepository schemaRepository)
        {
            var jsonDictionaryContract = (JsonDictionaryContract)_jsonContractResolver.ResolveContract(type);

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
                            name => _schemaGenerator.GenerateSchemaFor(valueType, schemaRepository)
                        )
                };
            }

            return new OpenApiSchema
            {
                Type = "object",
                AdditionalPropertiesAllowed = true,
                AdditionalProperties = _schemaGenerator.GenerateSchemaFor(valueType, schemaRepository)
            };
        }
    }
}