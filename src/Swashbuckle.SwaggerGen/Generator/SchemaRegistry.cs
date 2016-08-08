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

        class ReferencedType
        {
            public Type Type;
            public string SchemaId;
            public Schema Schema;
        }
        private IList<ReferencedType> _referencedTypes;

        public SchemaRegistry(
            JsonSerializerSettings jsonSerializerSettings,
            SchemaRegistryOptions options = null)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _jsonContractResolver = _jsonSerializerSettings.ContractResolver ?? new DefaultContractResolver();
            _options = options ?? new SchemaRegistryOptions();
            _referencedTypes = new List<ReferencedType>();
            Definitions = new Dictionary<string, Schema>();
        }

        public IDictionary<string, Schema> Definitions { get; private set; }

        public Schema GetOrRegister(Type type)
        {
            var schema = CreateSchema(type);

            // Iterate referenced types and ensure Schemas have been generated and added to Definitions
            ReferencedType todo;
            while ((todo = _referencedTypes.FirstOrDefault(rt => rt.Schema == null)) != null)
            {
                todo.Schema = CreateInlineSchema(todo.Type);
                Definitions.Add(todo.SchemaId, todo.Schema);
            }

            return schema;
        }

        private Schema CreateSchema(Type type)
        {
            var jsonContract = _jsonContractResolver.ResolveContract(type);

            var createReference = !_options.CustomTypeMappings.ContainsKey(type)
                && type != typeof(object)
                && (jsonContract is JsonObjectContract || jsonContract.IsSelfReferencingArrayOrDictionary());

            return createReference ? CreateReferenceSchema(type) : CreateInlineSchema(type);
        }

        private Schema CreateReferenceSchema(Type type)
        {
            var referencedType = _referencedTypes.FirstOrDefault(rt => rt.Type == type);
            if (referencedType == null)
            {
                referencedType = new ReferencedType { Type = type, SchemaId = GetSchemaIdFor(type) };
                _referencedTypes.Add(referencedType);
            }

            return new Schema { Ref = "#/definitions/" + referencedType.SchemaId };
        }

        private string GetSchemaIdFor(Type type)
        {
            var schemaId = _options.SchemaIdSelector(type);

            var conflict = _referencedTypes.FirstOrDefault(rt => rt.Type != type && rt.SchemaId == schemaId);
            if (conflict != null)
                throw new InvalidOperationException(string.Format(
                    "Conflicting schemaIds: Identical schemaIds detected for types {0} and {1}. " +
                    "See config settings - \"UseFullTypeNameInSchemaIds\" or \"CustomSchemaIds\" for a workaround",
                    type.FullName, conflict.Type.FullName));

            return schemaId;
        }

        private Schema CreateInlineSchema(Type type)
        {
            Schema schema;

            var jsonContract = _jsonContractResolver.ResolveContract(type);

            if (_options.CustomTypeMappings.ContainsKey(type))
            {
                schema = _options.CustomTypeMappings[type]();
            }
            else
            {
                // TODO: Perhaps a "Chain of Responsibility" would clean this up a little?
                if (jsonContract is JsonPrimitiveContract)
                    schema = CreatePrimitiveSchema((JsonPrimitiveContract)jsonContract);
                else if (jsonContract is JsonDictionaryContract)
                    schema = CreateDictionarySchema((JsonDictionaryContract)jsonContract);
                else if (jsonContract is JsonArrayContract)
                    schema = CreateArraySchema((JsonArrayContract)jsonContract);
                else if (jsonContract is JsonObjectContract && type != typeof(object))
                    schema = CreateObjectSchema((JsonObjectContract)jsonContract);
                else
                    // None of the above, fallback to abstract "object"
                    schema = new Schema { Type = "object" };
            }

            var filterContext = new SchemaFilterContext(type, jsonContract, this);
            foreach (var filter in _options.SchemaFilters)
            {
                filter.Apply(schema, filterContext);
            }

            return schema;
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
            var keyType = dictionaryContract.DictionaryKeyType ?? typeof(object);
            var valueType = dictionaryContract.DictionaryValueType ?? typeof(object);

            if (keyType.GetTypeInfo().IsEnum)
            {
                return new Schema
                {
                    Type = "object",
                    Properties = Enum.GetNames(keyType).ToDictionary(
                        (name) => dictionaryContract.DictionaryKeyResolver(name),
                        (name) => CreateSchema(valueType)
                    )
                };
            }
            else
            {
                return new Schema
                {
                    Type = "object",
                    AdditionalProperties = CreateSchema(valueType)
                };
            }
        }

        private Schema CreateArraySchema(JsonArrayContract arrayContract)
        {
            var itemType = arrayContract.CollectionItemType ?? typeof(object);
            return new Schema
            {
                Type = "array",
                Items = CreateSchema(itemType)
            };
        }

        private Schema CreateObjectSchema(JsonObjectContract jsonContract)
        {
            var properties = jsonContract.Properties
                .Where(p => !p.Ignored)
                .Where(p => !(_options.IgnoreObsoleteProperties && p.IsObsolete()))
                .ToDictionary(
                    prop => prop.PropertyName,
                    prop => CreateSchema(prop.PropertyType).AssignValidationProperties(prop)
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
            { typeof(byte[]), () => new Schema { Type = "string", Format = "byte" } },
            { typeof(sbyte[]), () => new Schema { Type = "string", Format = "byte" } },
            { typeof(bool), () => new Schema { Type = "boolean" } },
            { typeof(DateTime), () => new Schema { Type = "string", Format = "date-time" } },
            { typeof(DateTimeOffset), () => new Schema { Type = "string", Format = "date-time" } },
            { typeof(Guid), () => new Schema { Type = "string", Format = "uuid" } }
        };
    }
}