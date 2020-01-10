using System.Text.Json;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class JsonSchemaGenerator : SchemaGeneratorBase
    {
        public JsonSchemaGenerator(SchemaGeneratorOptions generatorOptions, JsonSerializerOptions serializerOptions)
            : base(generatorOptions)
        {
            AddHandler(new FileTypeHandler());
            AddHandler(new JsonEnumHandler(generatorOptions, serializerOptions));
            AddHandler(new JsonPrimitiveHandler());
            AddHandler(new JsonDictionaryHandler(this));
            AddHandler(new JsonArrayHandler(this));
            AddHandler(new JsonObjectHandler(generatorOptions, serializerOptions, this));
        }
    }
}
