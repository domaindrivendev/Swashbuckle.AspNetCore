using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Swashbuckle.Swagger
{
    public class SchemaGenerator : ISchemaRegistry
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly SchemaGeneratorOptions _options;

        private readonly IContractResolver _jsonContractResolver;
        private IDictionary<Type, SchemaInfo> referencedTypeMap;

        private class SchemaInfo
        {
            public string SchemaId;
            public Schema Schema;
        } 

        public SchemaGenerator(
            JsonSerializerSettings jsonSerializerSettings,
            SchemaGeneratorOptions options = null)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _options = options ?? new SchemaGeneratorOptions();

            _jsonContractResolver = _jsonSerializerSettings.ContractResolver ?? new DefaultContractResolver();
            referencedTypeMap = new Dictionary<Type, SchemaInfo>();
            Definitions = new Dictionary<string, Schema>();
        }

        public Schema GetOrRegister(Type type)
        {
            var schema = CreateSchema(type, true);

            // A null Schema in the type map indicates a Type that has been deferred
            while (referencedTypeMap.Any(entry => entry.Value.Schema == null))
            {
                var mapping = referencedTypeMap.First(entry => entry.Value.Schema == null);
                var schemaInfo = mapping.Value;

                schemaInfo.Schema = CreateSchema(mapping.Key);
                Definitions.Add(schemaInfo.SchemaId, schemaInfo.Schema);
            }

            return schema;
        }

        public IDictionary<string, Schema> Definitions { get; private set; }

        private Schema CreateSchema(Type type, bool refIfComplex = false)
        {
            if (_options.CustomTypeMappings.ContainsKey(type))
                return _options.CustomTypeMappings[type]();

            var jsonContract = _jsonContractResolver.ResolveContract(type);

            if (jsonContract is JsonPrimitiveContract)
                return CreatePrimitiveSchema((JsonPrimitiveContract)jsonContract);

            var dictionaryContract = jsonContract as JsonDictionaryContract;
            if (dictionaryContract != null)
                return dictionaryContract.IsSelfReferencing() && refIfComplex
                    ? CreateJsonReference(type)
                    : CreateDictionarySchema(dictionaryContract);

            var arrayContract = jsonContract as JsonArrayContract;
            if (arrayContract != null)
                return arrayContract.IsSelfReferencing() && refIfComplex
                    ? CreateJsonReference(type)
                    : CreateArraySchema(arrayContract);

            var objectContract = jsonContract as JsonObjectContract;
            if (objectContract != null)
                return refIfComplex
                    ? CreateJsonReference(type)
                    : CreateObjectSchema(objectContract);

            // None of the above, fallback to abstract "object"
            return CreateSchema(typeof(object), refIfComplex);
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

        private Schema CreateDictionarySchema(JsonDictionaryContract dictionaryContract)
        {
            var valueType = dictionaryContract.DictionaryValueType ?? typeof(object);
            return new Schema
                {
                    Type = "object",
                    AdditionalProperties = CreateSchema(valueType, true)
                };
        }

        private Schema CreateArraySchema(JsonArrayContract arrayContract)
        {
            var itemType = arrayContract.CollectionItemType ?? typeof(object);
            return new Schema
                {
                    Type = "array",
                    Items = CreateSchema(itemType, true)
                };
        }

        private Schema CreateObjectSchema(JsonObjectContract jsonContract)
        {
            var properties = jsonContract.Properties
                .Where(p => !p.Ignored)
                .Where(p => !(_options.IgnoreObsoleteProperties && p.IsObsolete()))
                .ToDictionary(
                    prop => prop.PropertyName,
                    prop => CreateSchema(prop.PropertyType, true).AssignValidationProperties(prop)
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

            var filterContext = new ModelFilterContext(jsonContract.UnderlyingType, jsonContract, this);
            foreach (var filter in _options.ModelFilters)
            {
                filter.Apply(schema, filterContext);
            }

            return schema;
        }

        private Schema CreateJsonReference(Type type)
        {
            if (!referencedTypeMap.ContainsKey(type))
            {
                var schemaId = type.FriendlyId(_options.UseFullTypeNameInSchemaIds);
                if (referencedTypeMap.Any(entry => entry.Value.SchemaId == schemaId))
                {
                    var conflictingType = referencedTypeMap.First(entry => entry.Value.SchemaId == schemaId).Key;
                    throw new InvalidOperationException(String.Format(
                        "Conflicting schemaIds: Duplicate schemaIds detected for types {0} and {1}. " +
                        "See the config setting - \"UseFullTypeNameInSchemaIds\" for a potential workaround",
                        type.FullName, conflictingType.FullName));
                }

                referencedTypeMap.Add(type, new SchemaInfo { SchemaId = schemaId });
            }

            return new Schema { Ref = "#/definitions/" + referencedTypeMap[type].SchemaId };
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
            { typeof(bool), () => new Schema { Type = "boolean" } },
            { typeof(DateTime), () => new Schema { Type = "string", Format = "date-time" } },
            { typeof(DateTimeOffset), () => new Schema { Type = "string", Format = "date-time" } }
        };
    }
}