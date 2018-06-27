using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaRegistry : ISchemaRegistry
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly IContractResolver _jsonContractResolver;
        private readonly SchemaRegistrySettings _settings;
        private readonly SchemaIdManager _schemaIdManager;
        private readonly IModelMetadataProvider _metadataProvider;
        public SchemaRegistry(
            JsonSerializerSettings jsonSerializerSettings,
            IModelMetadataProvider modelMetadataProvider,
            SchemaRegistrySettings settings = null)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _jsonContractResolver = _jsonSerializerSettings.ContractResolver ?? new DefaultContractResolver();
            _settings = settings ?? new SchemaRegistrySettings();
            _schemaIdManager = new SchemaIdManager(_settings.SchemaIdSelector);
            _metadataProvider = modelMetadataProvider;
            Definitions = new Dictionary<string, Schema>();
        }

        public IDictionary<string, Schema> Definitions { get; private set; }


        public Schema GetOrRegister(Type type)
        {
            var referencedModelMetadata = new Queue<ModelMetadata>();
            var metadata = _metadataProvider.GetMetadataForType(type);
            var schema = CreateSchema(metadata, referencedModelMetadata);

            // Ensure all referenced types have a corresponding definition
            while (referencedModelMetadata.Any())
            {
                var referencedType = referencedModelMetadata.Dequeue();
                var schemaId = _schemaIdManager.IdFor(referencedType.ModelType);
                if (Definitions.ContainsKey(schemaId)) continue;

                // NOTE: Add the schemaId first with a null value. This indicates a work-in-progress
                // and prevents a stack overflow by ensuring the above condition is met if the same
                // type ends up back on the referencedTypes queue via recursion within 'CreateInlineSchema'
                Definitions.Add(schemaId, null);
                Definitions[schemaId] = CreateInlineSchema(referencedType, referencedModelMetadata);
            }

            return schema;
        }

        private Schema CreateSchema(ModelMetadata modelMetaData, Queue<ModelMetadata> referencedMetadata)
        {
            // If Option<T> (F#), use the type argument
            if (modelMetaData.ModelType.IsFSharpOption())
            {
                var genericModelType = modelMetaData.ModelType.GetGenericArguments()[0];
                modelMetaData = _metadataProvider.GetMetadataForType(genericModelType);
            }

            var jsonContract = _jsonContractResolver.ResolveContract(modelMetaData.ModelType);

            var createReference = !_settings.CustomTypeMappings.ContainsKey(modelMetaData.ModelType)
                && modelMetaData.ModelType != typeof(object)
                && (// Type describes an object
                    jsonContract is JsonObjectContract ||
                    // Type is self-referencing
                    jsonContract.IsSelfReferencingArrayOrDictionary() ||
                    // Type is enum and opt-in flag set
                    (modelMetaData.ModelType.GetTypeInfo().IsEnum && _settings.UseReferencedDefinitionsForEnums));

            return createReference
                ? CreateReferenceSchema(modelMetaData, referencedMetadata)
                : CreateInlineSchema(modelMetaData, referencedMetadata);
        }

        private Schema CreateReferenceSchema(ModelMetadata modelMetaData, Queue<ModelMetadata> referencedMetadata)
        {
            referencedMetadata.Enqueue(modelMetaData);
            return new Schema { Ref = "#/definitions/" + _schemaIdManager.IdFor(modelMetaData.ModelType) };
        }

        private Schema CreateInlineSchema(ModelMetadata modelMetaData, Queue<ModelMetadata> referencedMetadata)
        {
            Schema schema;

            var jsonContract = _jsonContractResolver.ResolveContract(modelMetaData.ModelType);

            if (_settings.CustomTypeMappings.ContainsKey(modelMetaData.ModelType))
            {
                schema = _settings.CustomTypeMappings[modelMetaData.ModelType]();
            }
            else
            {
                // TODO: Perhaps a "Chain of Responsibility" would clean this up a little?
                if (jsonContract is JsonPrimitiveContract)
                    schema = CreatePrimitiveSchema((JsonPrimitiveContract)jsonContract);
                else if (jsonContract is JsonDictionaryContract)
                    schema = CreateDictionarySchema((JsonDictionaryContract)jsonContract, referencedMetadata);
                else if (jsonContract is JsonArrayContract)
                    schema = CreateArraySchema((JsonArrayContract)jsonContract, referencedMetadata);
                else if (jsonContract is JsonObjectContract && modelMetaData.ModelType != typeof(object))
                    schema = CreateObjectSchema((JsonObjectContract)jsonContract, referencedMetadata);
                else
                    // None of the above, fallback to abstract "object"
                    schema = new Schema { Type = "object" };
            }

            var filterContext = new SchemaFilterContext(modelMetaData.ModelType, modelMetaData, jsonContract, this);
            foreach (var filter in _settings.SchemaFilters)
            {
                filter.Apply(schema, filterContext);
            }

            return schema;
        }

        private Schema CreatePrimitiveSchema(JsonPrimitiveContract primitiveContract)
        {
            // If Nullable<T>, use the type argument
            var type = primitiveContract.UnderlyingType.IsNullable()
                ? Nullable.GetUnderlyingType(primitiveContract.UnderlyingType)
                : primitiveContract.UnderlyingType;

            if (type.GetTypeInfo().IsEnum)
                return CreateEnumSchema(primitiveContract, type);

            if (PrimitiveTypeMap.ContainsKey(type))
                return PrimitiveTypeMap[type]();

            // None of the above, fallback to string
            return new Schema { Type = "string" };
        }

        private Schema CreateEnumSchema(JsonPrimitiveContract primitiveContract, Type type)
        {
            var stringEnumConverter = primitiveContract.Converter as StringEnumConverter
                ?? _jsonSerializerSettings.Converters.OfType<StringEnumConverter>().FirstOrDefault();

            if (_settings.DescribeAllEnumsAsStrings || stringEnumConverter != null)
            {
                var camelCase = _settings.DescribeStringEnumsInCamelCase
                    || (stringEnumConverter != null && stringEnumConverter.CamelCaseText);

                var enumNames = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Select(f =>
                    {
                        var name = f.Name;

                        var enumMemberAttribute = f.GetCustomAttributes().OfType<EnumMemberAttribute>().FirstOrDefault();
                        if (enumMemberAttribute != null && enumMemberAttribute.Value != null)
                        {
                            name = enumMemberAttribute.Value;
                        }

                        return camelCase ? name.ToCamelCase() : name;
                    });

                return new Schema
                {
                    Type = "string",
                    Enum = enumNames.ToArray()
                };
            }

            return new Schema
            {
                Type = "integer",
                Format = "int32",
                Enum = Enum.GetValues(type).Cast<object>().ToArray()
            };
        }

        private Schema CreateDictionarySchema(JsonDictionaryContract dictionaryContract, Queue<ModelMetadata> referencedMetadata)
        {
            var keyType = dictionaryContract.DictionaryKeyType ?? typeof(object);
            var valueType = dictionaryContract.DictionaryValueType ?? typeof(object);

            if (keyType.GetTypeInfo().IsEnum)
            {
                return new Schema
                {
                    Type = "object",
                    Properties = Enum.GetNames(keyType).ToDictionary(
                        (name) => dictionaryContract.DictionaryKeyResolver(name),
                        (name) =>
                        {
                            var metadata = _metadataProvider.GetMetadataForType(valueType);
                            return CreateSchema(metadata, referencedMetadata);
                        }
                    )
                };
            }
            else
            {
                var metadata = _metadataProvider.GetMetadataForType(valueType);
                return new Schema
                {
                    Type = "object",
                    AdditionalProperties = CreateSchema(metadata, referencedMetadata)
                };
            }
        }

        private Schema CreateArraySchema(JsonArrayContract arrayContract, Queue<ModelMetadata> referencedMetadata)
        {
            var type = arrayContract.UnderlyingType;
            var itemType = arrayContract.CollectionItemType ?? typeof(object);

            var isASet = (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(ISet<>)
                || type.GetInterfaces().Any(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>)));

            var metadata = _metadataProvider.GetMetadataForType(itemType);
            return new Schema
            {
                Type = "array",
                Items = CreateSchema(metadata, referencedMetadata),
                UniqueItems = isASet
            };
        }

        private Schema CreateObjectSchema(JsonObjectContract jsonContract, Queue<ModelMetadata> referencedMetadata)
        {
            var applicableJsonProperties = jsonContract.Properties
                .Where(prop => !prop.Ignored)
                .Where(prop => !(_settings.IgnoreObsoleteProperties && prop.IsObsolete()))
                .Select(prop => prop);

            var required = applicableJsonProperties
                .Where(prop => prop.IsRequired())
                .Select(propInfo => propInfo.PropertyName)
                .ToList();

            var hasExtensionData = jsonContract.ExtensionDataValueType != null;

            var properties = applicableJsonProperties
                .ToDictionary(
                    prop => prop.PropertyName,
                    prop => CreatePropertySchema(prop, referencedMetadata));

            var schema = new Schema
            {
                Required = required.Any() ? required : null, // required can be null but not empty
                Properties = properties,
                AdditionalProperties = hasExtensionData ? new Schema { Type = "object" } : null,
                Type = "object",
            };

            return schema;
        }

        private Schema CreatePropertySchema(JsonProperty jsonProperty, Queue<ModelMetadata> referencedTypes)
        {
            var metadata = _metadataProvider.GetMetadataForType(jsonProperty.PropertyType);

            var schema = CreateSchema(metadata, referencedTypes);

            if (!jsonProperty.Writable)
                schema.ReadOnly = true;

            if (jsonProperty.TryGetMemberInfo(out MemberInfo memberInfo))
                schema.AssignAttributeMetadata(memberInfo.GetCustomAttributes(true));

            return schema;
        }

        private static readonly Dictionary<Type, Func<Schema>> PrimitiveTypeMap = new Dictionary<Type, Func<Schema>>
        {
            { typeof(short), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(ushort), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(int), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(uint), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(long), () => new Schema { Type = "integer", Format = "int64" } },
            { typeof(ulong), () => new Schema { Type = "integer", Format = "int64" } },
            { typeof(float), () => new Schema { Type = "number", Format = "float" } },
            { typeof(double), () => new Schema { Type = "number", Format = "double" } },
            { typeof(decimal), () => new Schema { Type = "number", Format = "double" } },
            { typeof(byte), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(sbyte), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(byte[]), () => new Schema { Type = "string", Format = "byte" } },
            { typeof(sbyte[]), () => new Schema { Type = "string", Format = "byte" } },
            { typeof(bool), () => new Schema { Type = "boolean" } },
            { typeof(DateTime), () => new Schema { Type = "string", Format = "date-time" } },
            { typeof(DateTimeOffset), () => new Schema { Type = "string", Format = "date-time" } },
            { typeof(Guid), () => new Schema { Type = "string", Format = "uuid" } }
        };
    }
}