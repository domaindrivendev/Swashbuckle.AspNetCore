using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Swashbuckle.Swagger.Model;

namespace Swashbuckle.SwaggerGen.Generator
{
    public class SchemaRegistry : ISchemaRegistry
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly IContractResolver _jsonContractResolver;
        private readonly SchemaRegistryOptions _options;
        private readonly IDictionary<string, Type> _referencedTypeMap;

        public SchemaRegistry(
            JsonSerializerSettings jsonSerializerSettings,
            SchemaRegistryOptions options = null)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _jsonContractResolver = _jsonSerializerSettings.ContractResolver ?? new DefaultContractResolver();
            _options = options ?? new SchemaRegistryOptions();
            _referencedTypeMap = new Dictionary<string, Type>();
            Definitions = new Dictionary<string, Schema>();
        }

        public IDictionary<string, Schema> Definitions { get; private set; }

        public Schema GetOrRegister(Type type)
        {
            var schema = CreateInlineSchema(type);

            // Ensure a corresponding definition exists for all referenced types
            string pendingSchemaId;
            while ((pendingSchemaId = GetPendingSchemaIds().FirstOrDefault()) != null)
            {
                Definitions.Add(pendingSchemaId, CreateDefinitionSchema(_referencedTypeMap[pendingSchemaId]));
            }

            return schema;
        }

        private Schema CreateInlineSchema(Type type)
        {
            if (_options.CustomTypeMappings.ContainsKey(type))
                return _options.CustomTypeMappings[type]();

            var jsonContract = _jsonContractResolver.ResolveContract(type);

            if (jsonContract is JsonPrimitiveContract)
                return CreatePrimitiveSchema((JsonPrimitiveContract)jsonContract);

            var dictionaryContract = jsonContract as JsonDictionaryContract;
            if (dictionaryContract != null)
                return dictionaryContract.IsSelfReferencing()
                    ? CreateRefSchema(type)
                    : CreateDictionarySchema(dictionaryContract);

            var arrayContract = jsonContract as JsonArrayContract;
            if (arrayContract != null)
                return arrayContract.IsSelfReferencing()
                    ? CreateRefSchema(type)
                    : CreateArraySchema(arrayContract);

            var objectContract = jsonContract as JsonObjectContract;
            if (objectContract != null && !objectContract.IsAmbiguous())
                return CreateRefSchema(type);

            // None of the above, fallback to abstract "object"
            return new Schema { Type = "object" };
        }

        private Schema CreatePrimitiveSchema(JsonPrimitiveContract primitiveContract)
        {
            var type = Nullable.GetUnderlyingType(primitiveContract.UnderlyingType)
                ?? primitiveContract.UnderlyingType;

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

            if (_options.DescribeAllEnumsAsStrings || stringEnumConverter != null)
            {
                var camelCase = _options.DescribeStringEnumsInCamelCase
                    || (stringEnumConverter != null && stringEnumConverter.CamelCaseText);

                return new Schema
                {
                    Type = "string",
                    Enum = (camelCase)
                        ? Enum.GetNames(type).Select(name => name.ToCamelCase()).ToArray()
                        : Enum.GetNames(type)
                };
            }

            return new Schema
            {
                Type = "integer",
                Format = "int32",
                Enum = Enum.GetValues(type).Cast<object>().ToArray()
            };
        }

        private Schema CreateRefSchema(Type type)
        {
            var schemaId = _options.SchemaIdSelector(type);

            if (_referencedTypeMap.ContainsKey(schemaId) && _referencedTypeMap[schemaId] != type)
                throw new InvalidOperationException(string.Format(
                    "Conflicting schemaIds: Duplicate schemaIds detected for types {0} and {1}. " +
                    "See the config setting - \"UseFullTypeNameInSchemaIds\" for a potential workaround",
                    type.FullName, _referencedTypeMap[schemaId].FullName));

            if (!_referencedTypeMap.ContainsKey(schemaId))
                _referencedTypeMap.Add(schemaId, type);

            return new Schema { Ref = "#/definitions/" + schemaId };
        }

        private Schema CreateDictionarySchema(JsonDictionaryContract dictionaryContract)
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
                        (name) => CreateInlineSchema(valueType)
                    )
                };
            }
            else
            {
                return new Schema
                {
                    Type = "object",
                    AdditionalProperties = CreateInlineSchema(valueType)
                };
            }
        }

        private Schema CreateArraySchema(JsonArrayContract arrayContract)
        {
            var itemType = arrayContract.CollectionItemType ?? typeof(object);
            return new Schema
                {
                    Type = "array",
                    Items = CreateInlineSchema(itemType)
                };
        }

        private Schema CreateDefinitionSchema(Type type)
        {
            var jsonContract = _jsonContractResolver.ResolveContract(type);

            if (jsonContract is JsonDictionaryContract)
                return CreateDictionarySchema((JsonDictionaryContract)jsonContract);

            if (jsonContract is JsonArrayContract)
                return CreateArraySchema((JsonArrayContract)jsonContract);

            if (jsonContract is JsonObjectContract)
                return CreateObjectSchema((JsonObjectContract)jsonContract);

            throw new InvalidOperationException(
                string.Format("Unsupported type - {0} for Defintitions. Must be Dictionary, Array or Object", type));
        }

        private Schema CreateObjectSchema(JsonObjectContract jsonContract)
        {
            var properties = jsonContract.Properties
                .Where(p => !p.Ignored)
                .Where(p => !(_options.IgnoreObsoleteProperties && p.IsObsolete()))
                .ToDictionary(
                    prop => prop.PropertyName,
                    prop => CreateInlineSchema(prop.PropertyType).AssignValidationProperties(prop)
                );

            var required = jsonContract.Properties.Where(prop => prop.IsRequired())
                .Select(propInfo => propInfo.PropertyName)
                .ToList();

            var schema = new Schema
            {
                Required = required.Any() ? required : null, // required can be null but not empty
                Properties = properties,
                Type = "object"
            };

            var filterContext = new ModelFilterContext(
                jsonContract.UnderlyingType,
                jsonContract,
                this);

            foreach (var filter in _options.ModelFilters)
            {
                filter.Apply(schema, filterContext);
            }

            return schema;
        }

        private IEnumerable<string> GetPendingSchemaIds()
        {
            var referenced = _referencedTypeMap.Keys;
            var defined = Definitions.Keys;
            return referenced.Except(defined);
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
            { typeof(byte), () => new Schema { Type = "string", Format = "byte" } },
            { typeof(sbyte), () => new Schema { Type = "string", Format = "byte" } },
            { typeof(byte[]), () => new Schema { Type = "string", Format = "byte" } },
            { typeof(sbyte[]), () => new Schema { Type = "string", Format = "byte" } },
            { typeof(bool), () => new Schema { Type = "boolean" } },
            { typeof(DateTime), () => new Schema { Type = "string", Format = "date-time" } },
            { typeof(DateTimeOffset), () => new Schema { Type = "string", Format = "date-time" } },
            { typeof(Guid), () => new Schema { Type = "string", Format = "uuid" } }
        };
    }
}