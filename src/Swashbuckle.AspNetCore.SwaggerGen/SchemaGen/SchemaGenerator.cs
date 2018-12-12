using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaGenerator : ISchemaGenerator
    {
        private readonly SchemaGeneratorOptions _options;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly IContractResolver _jsonContractResolver;
        private readonly ISchemaGenerator[] _subGenerators;

        public SchemaGenerator(IOptions<MvcJsonOptions> jsonOptionsAccessor, IOptions<SchemaGeneratorOptions> optionsAccessor)
            : this(jsonOptionsAccessor.Value?.SerializerSettings, optionsAccessor.Value)
        { }

        public SchemaGenerator(JsonSerializerSettings jsonSerializerSettings, SchemaGeneratorOptions options)
        {
            // NOTE: An OpenApiSchema MAY be used to describe both JSON and non-JSON media types. However, this implementation of ISchemaGenerator is optimized
            // for JSON and therefore couples to several JSON.NET abstractions so that it can provide accurate descriptions of types in their serialized form.

            _jsonSerializerSettings = jsonSerializerSettings ?? new JsonSerializerSettings();
            _jsonContractResolver = _jsonSerializerSettings.ContractResolver ?? new DefaultContractResolver();
            _options  = options ?? new SchemaGeneratorOptions();

            _subGenerators = new ISchemaGenerator[]
            {
                new PolymorphicSchemaGenerator(_options, this),
                new JsonPrimitiveSchemaGenerator(_options, _jsonContractResolver, _jsonSerializerSettings),
                new JsonDictionarySchemaGenerator(_options, _jsonContractResolver, this),
                new JsonArraySchemaGenerator(_options, _jsonContractResolver, this),
                new JsonObjectSchemaGenerator(_options, _jsonContractResolver, this),
            };
        }

        public bool CanGenerateSchemaFor(Type type) => true;

        public OpenApiSchema GenerateSchemaFor(Type type, SchemaRepository schemaRepository)
        {
            if (_options.CustomTypeMappings.ContainsKey(type))
                return _options.CustomTypeMappings[type]();

            if (KnownTypeMappings.ContainsKey(type))
                return KnownTypeMappings[type]();

            if (type.IsNullable() || type.IsFSharpOption()) // unwrap nullables
                type = type.GenericTypeArguments[0];

            var jsonContract = _jsonContractResolver.ResolveContract(type);

            if (type.IsEnum // enum
                || (jsonContract is JsonObjectContract) // regular object
                || (jsonContract is JsonArrayContract && ((JsonArrayContract)jsonContract).CollectionItemType == jsonContract.UnderlyingType) // self-referencing array
                || (jsonContract is JsonDictionaryContract && ((JsonDictionaryContract)jsonContract).DictionaryValueType == jsonContract.UnderlyingType)) // self-referencing dictionary
            {
                return GenerateReferenceSchemaFor(type, schemaRepository, jsonContract);
            }

            return GenerateDefinitionSchemaFor(type, schemaRepository, jsonContract);
        }

        private OpenApiSchema GenerateReferenceSchemaFor(Type type, SchemaRepository schemaRepository, JsonContract jsonContract)
        {
            if (!schemaRepository.TryGetIdFor(type, out string schemaId))
            {
                schemaId = _options.SchemaIdSelector(type);
                schemaRepository.ReserveIdFor(type, schemaId);

                schemaRepository.AddSchemaFor(type, GenerateDefinitionSchemaFor(type, schemaRepository, jsonContract));
            }

            return new OpenApiSchema
            {
                Reference = new OpenApiReference { Id = schemaId, Type = ReferenceType.Schema }
            };
        }

        private OpenApiSchema GenerateDefinitionSchemaFor(Type type, SchemaRepository schemaRepository, JsonContract jsonContract)
        {
            OpenApiSchema schema = null;

            foreach (var subGenerator in _subGenerators)
            {
                if (!subGenerator.CanGenerateSchemaFor(type)) continue;

                schema = subGenerator.GenerateSchemaFor(type, schemaRepository);
                ApplyFiltersTo(schema, type, schemaRepository, jsonContract);
                return schema;
            }

            throw new InvalidOperationException("TODO:");
        }

        private void ApplyFiltersTo(OpenApiSchema schema, Type type, SchemaRepository schemaRepository, JsonContract jsonContract)
        {
            var filterContext = new SchemaFilterContext(type, jsonContract, schemaRepository, this);
            foreach (var filter in _options.SchemaFilters)
            {
                filter.Apply(schema, filterContext);
            }
        }

        private static Dictionary<Type, Func<OpenApiSchema>> KnownTypeMappings = new Dictionary<Type, Func<OpenApiSchema>>
        {
            { typeof(object), () => new OpenApiSchema { Type = "object" } },
            { typeof(JToken), () => new OpenApiSchema { Type = "object" } },
            { typeof(JObject), () => new OpenApiSchema { Type = "object" } }
        };
    }
}