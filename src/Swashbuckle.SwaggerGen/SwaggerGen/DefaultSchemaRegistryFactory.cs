using Newtonsoft.Json;

namespace Swashbuckle.SwaggerGen
{
    public class DefaultSchemaRegistryFactory : ISchemaRegistryFactory
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly SwaggerSchemaOptions _options;

        public DefaultSchemaRegistryFactory(
            JsonSerializerSettings jsonSerializerSettings,
            SwaggerSchemaOptions options)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _options = options;
        }

        public ISchemaRegistry Create()
        {
            return new DefaultSchemaRegistry(_jsonSerializerSettings, _options);
        }
    }
}
