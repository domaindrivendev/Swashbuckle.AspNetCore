using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Newtonsoft
{
    public class NewtonsoftSchemaGenerator : SchemaGeneratorBase
    {
        public NewtonsoftSchemaGenerator(SchemaGeneratorOptions generatorOptions, JsonSerializerSettings serializerSettings)
            : base(generatorOptions)
        {
            var contractResolver = serializerSettings.ContractResolver ?? new DefaultContractResolver();

            AddHandler(new FileTypeHandler());
            AddHandler(new PolymorphicTypeHandler(generatorOptions, this));
            AddHandler(new PrimitiveTypeHandler());
            AddHandler(new NewtonsoftLinqTypeHandler());
            AddHandler(new NewtonsoftEnumHandler(generatorOptions, contractResolver, serializerSettings));
            AddHandler(new NewtonsoftDictionaryHandler(contractResolver, this));
            AddHandler(new NewtonsoftArrayHandler(contractResolver, this));
            AddHandler(new NewtonsoftObjectHandler(generatorOptions, contractResolver, this));
        }
    }
}
