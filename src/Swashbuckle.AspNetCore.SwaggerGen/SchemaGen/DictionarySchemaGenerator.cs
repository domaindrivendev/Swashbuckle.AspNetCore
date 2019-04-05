using System;
using System.Linq;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class DictionarySchemaGenerator : ChainableSchemaGenerator
    {
        public DictionarySchemaGenerator(
            IContractResolver contractResolver,
            ISchemaGenerator rootGenerator,
            SchemaGeneratorOptions options)
            : base(contractResolver, rootGenerator, options)
        { }

        protected override bool CanGenerateSchemaFor(Type type)
        {
            return ContractResolver.ResolveContract(type) is JsonDictionaryContract;
        }

        protected override OpenApiSchema GenerateSchemaFor(Type type, SchemaRepository schemaRepository)
        {
            var jsonDictionaryContract = (JsonDictionaryContract)ContractResolver.ResolveContract(type);

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
                            name => RootGenerator.GenerateSchema(valueType, schemaRepository)
                        )
                };
            }

            return new OpenApiSchema
            {
                Type = "object",
                AdditionalPropertiesAllowed = true,
                AdditionalProperties = RootGenerator.GenerateSchema(valueType, schemaRepository)
            };
        }
    }
}