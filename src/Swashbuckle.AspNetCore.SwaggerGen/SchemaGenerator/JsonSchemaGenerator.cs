using System.Text.Json;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class JsonSchemaGenerator : SchemaGeneratorBase
    {
        public JsonSchemaGenerator(SchemaGeneratorOptions generatorOptions, JsonSerializerOptions serializerOptions)
            : base(generatorOptions)
        {
            AddHandler(new FileTypeHandler());
            AddHandler(new PolymorphicTypeHandler(generatorOptions, this));
            AddHandler(new JsonPrimitiveTypeHandler(serializerOptions));
            AddHandler(new JsonEnumHandler(serializerOptions));
            AddHandler(new JsonDictionaryHandler(serializerOptions, this));
            AddHandler(new JsonArrayHandler(serializerOptions, this));
            AddHandler(new JsonObjectHandler(generatorOptions, serializerOptions, this));
        }
    }
}
