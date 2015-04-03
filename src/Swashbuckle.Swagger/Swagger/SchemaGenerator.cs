using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Swashbuckle.Swagger
{
    public class SchemaGenerator : ISchemaRegistry
    {
        private readonly IContractResolver _jsonContractResolver;
        private readonly SchemaGeneratorOptions _options;

        private IDictionary<Type, SchemaInfo> referencedTypeMap;
        private class SchemaInfo
        {
            public string SchemaId;
            public Schema Schema;
        } 

        public SchemaGenerator(
            IContractResolver jsonContractResolver,
            SchemaGeneratorOptions options = null)
        {
            _jsonContractResolver = jsonContractResolver;
            _options = options ?? new SchemaGeneratorOptions();

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

            if (jsonContract is JsonObjectContract && !jsonContract.IsDeterministic())
                return refIfComplex
                    ? CreateJsonReference(type)
                    : CreateObjectSchema((JsonObjectContract)jsonContract);

            // None of the above, fallback to abstract "object"
            return CreateSchema(typeof(object), refIfComplex);
        }

        private Schema CreatePrimitiveSchema(JsonPrimitiveContract primitiveContract)
        {
            var type = Nullable.GetUnderlyingType(primitiveContract.UnderlyingType)
                ?? primitiveContract.UnderlyingType;

            if (type.IsEnum)
            {
                var converter = primitiveContract.Converter;
                var describeAsString = _options.DescribeAllEnumsAsStrings
                    || (converter != null && converter.GetType() == typeof(StringEnumConverter));

                return describeAsString
                    ? new Schema { type = "string", @enum = type.GetEnumNames() }
                    : new Schema { type = "integer", format = "int32", @enum = type.GetEnumValues().Cast<object>().ToArray() };
            }

            if (PrimitiveTypeMap.ContainsKey(type))
                return PrimitiveTypeMap[type]();

            // None of the above, fallback to string
            return new Schema { type = "string" };
        }

        private Schema CreateDictionarySchema(JsonDictionaryContract dictionaryContract)
        {
            var valueType = dictionaryContract.DictionaryValueType ?? typeof(object);
            return new Schema
                {
                    type = "object",
                    additionalProperties = CreateSchema(valueType, true)
                };
        }

        private Schema CreateArraySchema(JsonArrayContract arrayContract)
        {
            var itemType = arrayContract.CollectionItemType ?? typeof(object);
            return new Schema
                {
                    type = "array",
                    items = CreateSchema(itemType, true)
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
                required = required.Any() ? required : null, // required can be null but not empty
                properties = properties,
                type = "object"
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
                var schemaId = SchemaIdFor(type);
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

            return new Schema { @ref = _options.SchemaReferencePrefix + referencedTypeMap[type].SchemaId };
        }

        public string SchemaIdFor(Type type)
        {
            var typeName = type.Name;
            if (_options.UseFullTypeNameInSchemaIds)
                typeName = type.Namespace + "." + typeName;

            if (type.IsGenericType)
            {
                var genericArgumentIds = type.GetGenericArguments()
                    .Select(t => SchemaIdFor(t))
                    .ToArray();

                return new StringBuilder(typeName)
                    .Replace(String.Format("`{0}", genericArgumentIds.Count()), String.Empty)
                    .Append(String.Format("[{0}]", String.Join(",", genericArgumentIds).TrimEnd(',')))
                    .ToString();
            }

            return typeName;
        }

        private static readonly Dictionary<Type, Func<Schema>> PrimitiveTypeMap = new Dictionary<Type, Func<Schema>>
        {
            { typeof(short), () => new Schema { type = "integer", format = "int32" } },
            { typeof(ushort), () => new Schema { type = "integer", format = "int32" } },
            { typeof(int), () => new Schema { type = "integer", format = "int32" } },
            { typeof(uint), () => new Schema { type = "integer", format = "int32" } },
            { typeof(long), () => new Schema { type = "integer", format = "int64" } },
            { typeof(ulong), () => new Schema { type = "integer", format = "int64" } },
            { typeof(float), () => new Schema { type = "number", format = "float" } },
            { typeof(double), () => new Schema { type = "number", format = "double" } },
            { typeof(decimal), () => new Schema { type = "number", format = "double" } },
            { typeof(byte), () => new Schema { type = "string", format = "byte" } },
            { typeof(sbyte), () => new Schema { type = "string", format = "byte" } },
            { typeof(bool), () => new Schema { type = "boolean" } },
            { typeof(DateTime), () => new Schema { type = "string", format = "date-time" } },
            { typeof(DateTimeOffset), () => new Schema { type = "string", format = "date-time" } }
        };
    }
}