using Newtonsoft.Json;

namespace Swashbuckle.SwaggerGen
{
    public class SchemaRegistryFactory : ISchemaRegistryFactory
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly SchemaRegistryOptions _options;

        public SchemaRegistryFactory(
            JsonSerializerSettings jsonSerializerSettings,
            SchemaRegistryOptions options)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _options = options;
        }

        public ISchemaRegistry Create()
        {
            return new SchemaRegistry(_jsonSerializerSettings, _options);
        }
    }
}
