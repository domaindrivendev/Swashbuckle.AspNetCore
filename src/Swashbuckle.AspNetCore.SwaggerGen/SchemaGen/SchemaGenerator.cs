using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaGenerator : ISchemaGenerator
    {
        private readonly IContractResolver _jsonContractResolver;
        private readonly SchemaGeneratorHandler _chainOfHandlers;

        public SchemaGenerator(
            IOptions<SchemaGeneratorOptions> schemaGeneratorOptionsAccessor,
            ISerializerSettingsAccessor jsonSerializerSettingsAccessor)
            : this(
                  schemaGeneratorOptionsAccessor.Value ?? new SchemaGeneratorOptions(),
                  jsonSerializerSettingsAccessor.Value ?? new JsonSerializerSettings())
        { }

        public SchemaGenerator(SchemaGeneratorOptions schemaGeneratorOptions, JsonSerializerSettings jsonSerializerSettings)
        {
            _jsonContractResolver = jsonSerializerSettings.ContractResolver ?? new DefaultContractResolver();

            // To manage complexity, implement as a chain of responsibility
            _chainOfHandlers = new KnownTypeHandler(schemaGeneratorOptions, this, jsonSerializerSettings)
                .Add(new ReferenceableTypeHandler(schemaGeneratorOptions, this, jsonSerializerSettings))
                .Add(new PolymorphicTypeHandler(schemaGeneratorOptions, this, jsonSerializerSettings))
                .Add(new JsonPrimitiveHandler(schemaGeneratorOptions, this, jsonSerializerSettings))
                .Add(new JsonArrayHandler(schemaGeneratorOptions, this, jsonSerializerSettings))
                .Add(new JsonDictionaryHandler(schemaGeneratorOptions, this, jsonSerializerSettings))
                .Add(new JsonObjectHandler(schemaGeneratorOptions, this, jsonSerializerSettings))
                .Add(new FallbackHandler(schemaGeneratorOptions, this, jsonSerializerSettings));
        }

        public OpenApiSchema GenerateSchema(ModelMetadata modelMetadata, SchemaRepository schemaRepository)
        {
            var jsonContract = _jsonContractResolver.ResolveContract(modelMetadata.UnderlyingOrModelType);

            return _chainOfHandlers.GenerateSchema(modelMetadata, schemaRepository, jsonContract);
        }
    }

    public abstract class SchemaGeneratorHandler
    {
        protected SchemaGeneratorHandler(
            SchemaGeneratorOptions schemaGeneratorOptions,
            ISchemaGenerator schemaGenerator,
            JsonSerializerSettings jsonSerializerSettings)
        {
            SchemaGeneratorOptions = schemaGeneratorOptions;
            SchemaGenerator = schemaGenerator;
            JsonSerializerSettings = jsonSerializerSettings;
        }

        protected SchemaGeneratorOptions SchemaGeneratorOptions { get; }
        protected ISchemaGenerator SchemaGenerator { get; }
        protected JsonSerializerSettings JsonSerializerSettings { get; }
        protected SchemaGeneratorHandler Next { get; set; }

        public OpenApiSchema GenerateSchema(ModelMetadata modelMetadata, SchemaRepository schemaRepository, JsonContract jsonContract)
        {
            if (CanGenerateSchemaFor(modelMetadata, jsonContract))
            {
                var schema = GenerateSchemaFor(modelMetadata, schemaRepository, jsonContract);

                if (schema.Reference == null)
                    ApplyFilters(schema, modelMetadata, schemaRepository, jsonContract);

                return schema;
            }

            return Next.GenerateSchema(modelMetadata, schemaRepository, jsonContract);
        }

        public SchemaGeneratorHandler Add(SchemaGeneratorHandler handler)
        {
            var tail = this;
            while (tail.Next != null) tail = tail.Next;
            tail.Next = handler;

            return this;
        }

        protected abstract bool CanGenerateSchemaFor(ModelMetadata modelMetadata, JsonContract jsonContract);

        protected abstract OpenApiSchema GenerateSchemaFor(ModelMetadata modelMetadata, SchemaRepository schemaRepository, JsonContract jsonContract);

        private void ApplyFilters(OpenApiSchema schema, ModelMetadata modelMetadata, SchemaRepository schemaRepository, JsonContract jsonContract)
        {
            var context = new SchemaFilterContext(modelMetadata, schemaRepository, SchemaGenerator, jsonContract);

            foreach (var filter in SchemaGeneratorOptions.SchemaFilters)
            {
                filter.Apply(schema, context);
            }
        }
    }
}
