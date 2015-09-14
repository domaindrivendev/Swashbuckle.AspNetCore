using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Swashbuckle.Swagger
{
    public class DefaultSchemaProvider : ISchemaProvider
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly IContractResolver _jsonContractResolver;
        private readonly SchemaGeneratorOptions _options;

        private struct ReferencedType
        {
            public string SchemaId;
            public Type Type;
        }

        public DefaultSchemaProvider(
            JsonSerializerSettings jsonSerializerSettings,
            SchemaGeneratorOptions options = null)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _jsonContractResolver = _jsonSerializerSettings.ContractResolver ?? new DefaultContractResolver();
            _options = options ?? new SchemaGeneratorOptions();
        }

        public Schema GetSchema(Type type, IDictionary<string, Schema> definitions)
        {
            var referencedTypes = new Queue<ReferencedType>();
            var schema = CreateSchema(type, true, referencedTypes);

            while (referencedTypes.Any())
            {
                var item = referencedTypes.Dequeue();
                if (definitions.ContainsKey(item.SchemaId)) continue;

                definitions.Add(item.SchemaId, CreateSchema(item.Type, false, referencedTypes));
            }

            return schema;
        }

        private Schema CreateSchema(Type type, bool refIfComplex, Queue<ReferencedType> referencedTypes)
        {
            if (_options.CustomTypeMappings.ContainsKey(type))
                return _options.CustomTypeMappings[type]();

            var jsonContract = _jsonContractResolver.ResolveContract(type);

            if (jsonContract is JsonPrimitiveContract)
                return CreatePrimitiveSchema((JsonPrimitiveContract)jsonContract);

            var dictionaryContract = jsonContract as JsonDictionaryContract;
            if (dictionaryContract != null)
                return dictionaryContract.IsSelfReferencing() && refIfComplex
                    ? CreateJsonReference(type, referencedTypes)
                    : CreateDictionarySchema(dictionaryContract, referencedTypes);

            var arrayContract = jsonContract as JsonArrayContract;
            if (arrayContract != null)
                return arrayContract.IsSelfReferencing() && refIfComplex
                    ? CreateJsonReference(type, referencedTypes)
                    : CreateArraySchema(arrayContract, referencedTypes);

            var objectContract = jsonContract as JsonObjectContract;
            if (objectContract != null)
                return refIfComplex
                    ? CreateJsonReference(type, referencedTypes)
                    : CreateObjectSchema(objectContract, referencedTypes);

            // None of the above, fallback to abstract "object"
            return CreateSchema(typeof(object), refIfComplex, referencedTypes);
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

        private Schema CreateJsonReference(Type type, Queue<ReferencedType> referencedTypes)
        {
            var schemaId = _options.SchemaIdSelector(type);

            if (!referencedTypes.Any(item => item.SchemaId == schemaId))
                referencedTypes.Enqueue(new ReferencedType { SchemaId = schemaId, Type = type });

            return new Schema { Ref = "#/definitions/" + schemaId };
        }

        private Schema CreateDictionarySchema(
            JsonDictionaryContract dictionaryContract,
            Queue<ReferencedType> referencedTypes)
        {
            var valueType = dictionaryContract.DictionaryValueType ?? typeof(object);
            return new Schema
                {
                    Type = "object",
                    AdditionalProperties = CreateSchema(valueType, true, referencedTypes)
                };
        }

        private Schema CreateArraySchema(
            JsonArrayContract arrayContract,
            Queue<ReferencedType> referencedTypes)
        {
            var itemType = arrayContract.CollectionItemType ?? typeof(object);
            return new Schema
                {
                    Type = "array",
                    Items = CreateSchema(itemType, true, referencedTypes)
                };
        }

        private Schema CreateObjectSchema(
            JsonObjectContract jsonContract,
            Queue<ReferencedType> referencedTypes)
        {
            var properties = jsonContract.Properties
                .Where(p => !p.Ignored)
                .Where(p => !(_options.IgnoreObsoleteProperties && p.IsObsolete()))
                .ToDictionary(
                    prop => prop.PropertyName,
                    prop => CreateSchema(prop.PropertyType, true, referencedTypes)
                        .AssignValidationProperties(prop)
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