using System;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaGenerator : ISchemaGenerator
    {
        private readonly SchemaGeneratorOptions _schemaGeneratorOptions;
        private readonly IContractResolver _jsonContractResolver;
        private readonly SchemaGeneratorHandler _chainOfHandlers;

        public SchemaGenerator(
            IOptions<SchemaGeneratorOptions> schemaGeneratorOptionsAccessor,
            ISerializerSettingsAccessor serializerSettingsAccessor)
            : this(
                  schemaGeneratorOptionsAccessor.Value ?? new SchemaGeneratorOptions(),
                  serializerSettingsAccessor.Value ?? new JsonSerializerSettings())
        { }

        public SchemaGenerator(SchemaGeneratorOptions schemaGeneratorOptions, JsonSerializerSettings jsonSerializerSettings)
        {
            _schemaGeneratorOptions = schemaGeneratorOptions;
            _jsonContractResolver = jsonSerializerSettings.ContractResolver ?? new DefaultContractResolver();

            _chainOfHandlers = new PolymorphicTypeHandler(schemaGeneratorOptions, this)
                .Add(new FileTypeHandler(schemaGeneratorOptions, this))
                .Add(new JsonPrimitiveHandler(schemaGeneratorOptions, this, jsonSerializerSettings))
                .Add(new JsonDictionaryHandler(schemaGeneratorOptions, this))
                .Add(new JsonArrayHandler(schemaGeneratorOptions, this))
                .Add(new JsonObjectHandler(schemaGeneratorOptions, this))
                .Add(new FallbackHandler(schemaGeneratorOptions, this));
        }

        public OpenApiSchema GenerateSchema(Type type, SchemaRepository schemaRepository)
        {
            if (_schemaGeneratorOptions.CustomTypeMappings.ContainsKey(type))
                return _schemaGeneratorOptions.CustomTypeMappings[type]();

            // Unwrap nullables before passing through the generator 
            var typeToResolve = type.IsNullable(out Type valueType) ? valueType : type;

            var jsonContract = _jsonContractResolver.ResolveContract(typeToResolve);

            return _chainOfHandlers.GenerateSchema(jsonContract, schemaRepository);
        }
    }

    public abstract class SchemaGeneratorHandler
    {
        protected SchemaGeneratorHandler(SchemaGeneratorOptions schemaGeneratorOptions, ISchemaGenerator schemaGenerator)
        {
            SchemaGeneratorOptions = schemaGeneratorOptions;
            SchemaGenerator = schemaGenerator;
        }

        protected SchemaGeneratorOptions SchemaGeneratorOptions { get; }
        protected ISchemaGenerator SchemaGenerator { get; }
        protected SchemaGeneratorHandler Next { get; set; }

        public OpenApiSchema GenerateSchema(JsonContract jsonContract, SchemaRepository schemaRepository)
        {
            if (!CanGenerateSchema(jsonContract, out bool shouldBeReferenced))
                return Next.GenerateSchema(jsonContract, schemaRepository);

            if (shouldBeReferenced)
            {
                var schemaId = SchemaGeneratorOptions.SchemaIdSelector(jsonContract.UnderlyingType);

                return schemaRepository.GetOrAdd(jsonContract.UnderlyingType,
                    schemaId,
                    () => GenerateDefinitionSchemaAndApplyFilters(jsonContract, schemaRepository));
            }

            return GenerateDefinitionSchemaAndApplyFilters(jsonContract, schemaRepository);
        }

        public SchemaGeneratorHandler Add(SchemaGeneratorHandler handler)
        {
            var tail = this;
            while (tail.Next != null) tail = tail.Next;
            tail.Next = handler;

            return this;
        }

        protected abstract bool CanGenerateSchema(JsonContract jsonContract, out bool shouldBeReferenced);

        protected abstract OpenApiSchema GenerateDefinitionSchema(JsonContract jsonContract, SchemaRepository schemaRepository);

        private OpenApiSchema GenerateDefinitionSchemaAndApplyFilters(JsonContract jsonContract, SchemaRepository schemaRepository)
        {
            var schema = GenerateDefinitionSchema(jsonContract, schemaRepository);

            var filterContext = new SchemaFilterContext(jsonContract, schemaRepository, SchemaGenerator);
            foreach (var filter in SchemaGeneratorOptions.SchemaFilters)
            {
                filter.Apply(schema, filterContext);
            }

            return schema;
        }
    }
}
