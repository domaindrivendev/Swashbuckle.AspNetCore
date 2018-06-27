using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaRegistryFactory : ISchemaRegistryFactory
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly SchemaRegistrySettings _schemaRegistrySettings;
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public SchemaRegistryFactory(
            JsonSerializerSettings jsonSerializerSettings,
            IModelMetadataProvider modelMetadataProvider,
            SchemaRegistrySettings schemaRegistrySettings)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _modelMetadataProvider = modelMetadataProvider;
            _schemaRegistrySettings = schemaRegistrySettings;
        }

        public ISchemaRegistry Create()
        {
            return new SchemaRegistry(_jsonSerializerSettings, _modelMetadataProvider, _schemaRegistrySettings);
        }
    }
}
