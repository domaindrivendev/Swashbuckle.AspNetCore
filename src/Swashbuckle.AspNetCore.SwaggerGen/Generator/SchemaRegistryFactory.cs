using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaRegistryFactory : ISchemaRegistryFactory
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly SchemaRegistryOptions _schemaRegistryOptions;

        public SchemaRegistryFactory(
            IOptions<MvcJsonOptions> mvcJsonOptionsAccessor,
            IOptions<SchemaRegistryOptions> schemaRegistryOptionsAccessor)
            : this(mvcJsonOptionsAccessor.Value.SerializerSettings, schemaRegistryOptionsAccessor.Value)
        { }

        public SchemaRegistryFactory(
            JsonSerializerSettings jsonSerializerSettings,
            SchemaRegistryOptions schemaRegistryOptions)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _schemaRegistryOptions = schemaRegistryOptions;
        }

        public ISchemaRegistry Create()
        {
            return new SchemaRegistry(_jsonSerializerSettings, _schemaRegistryOptions);
        }
    }
}
