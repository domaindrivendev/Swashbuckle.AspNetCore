using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaRegistry : ISchemaRegistry
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly IContractResolver _jsonContractResolver;
        private readonly SchemaRegistrySettings _settings;
        private readonly SchemaIdManager _schemaIdManager;

        public SchemaRegistry(
            JsonSerializerSettings jsonSerializerSettings,
            SchemaRegistrySettings settings = null)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _jsonContractResolver = _jsonSerializerSettings.ContractResolver ?? new DefaultContractResolver();
            _settings = settings ?? new SchemaRegistrySettings();
            _schemaIdManager = new SchemaIdManager(_settings.SchemaIdSelector);
            Definitions = new Dictionary<string, Schema>();
        }

        public IDictionary<string, Schema> Definitions { get; private set; }

        public Schema GetOrRegister(Type type)
        {
            var referencedTypes = new Queue<Type>();
            var schema = CreateSchema(type, referencedTypes);

            // Ensure all referenced types have a corresponding definition
            while (referencedTypes.Any())
            {
                var referencedType = referencedTypes.Dequeue();
                var schemaId = _schemaIdManager.IdFor(referencedType);
                if (Definitions.ContainsKey(schemaId)) continue;

                // NOTE: Add the schemaId first with a null value. This indicates a work-in-progress
                // and prevents a stack overflow by ensuring the above condition is met if the same
                // type ends up back on the referencedTypes queue via recursion within 'CreateInlineSchema'
                Definitions.Add(schemaId, null);
                Definitions[schemaId] = CreateInlineSchema(referencedType, referencedTypes);
            }

            return schema;
        }

        private Schema CreateSchema(Type type, Queue<Type> referencedTypes)
        {
            type = GetUnderlyingType(type);

            var jsonContract = _jsonContractResolver.ResolveContract(type);

            var createReference = !_settings.CustomTypeMappings.ContainsKey(type)
                && type != typeof(object)
                && (// Type describes an object
                    jsonContract is JsonObjectContract ||
                    // Type is self-referencing
                    jsonContract.IsSelfReferencingArrayOrDictionary() ||
                    // Type is enum and opt-in flag set
                    (type.IsEnumType() && _settings.UseReferencedDefinitionsForEnums));

            return createReference
                ? CreateReferenceSchema(type, referencedTypes)
                : CreateInlineSchema(type, referencedTypes);
        }

        private Schema CreateReferenceSchema(Type type, Queue<Type> referencedTypes)
        {
            referencedTypes.Enqueue(type);
            return new Schema { Ref = "#/definitions/" + _schemaIdManager.IdFor(type) };
        }

        private Schema CreateInlineSchema(Type type, Queue<Type> referencedTypes)
        {
            Schema schema;

            var jsonContract = _jsonContractResolver.ResolveContract(type);

            if (_settings.CustomTypeMappings.ContainsKey(type))
            {
                schema = _settings.CustomTypeMappings[type]();
            }
            else
            {
                // TODO: Perhaps a "Chain of Responsibility" would clean this up a little?
                if (jsonContract is JsonPrimitiveContract)
                    schema = CreatePrimitiveSchema((JsonPrimitiveContract)jsonContract);
                else if (jsonContract is JsonDictionaryContract)
                    schema = CreateDictionarySchema((JsonDictionaryContract)jsonContract, referencedTypes);
                else if (jsonContract is JsonArrayContract)
                    schema = CreateArraySchema((JsonArrayContract)jsonContract, referencedTypes);
                else if (jsonContract is JsonObjectContract && type != typeof(object))
                    schema = CreateObjectSchema((JsonObjectContract)jsonContract, referencedTypes);
                else
                    // None of the above, fallback to abstract "object"
                    schema = new Schema { Type = "object" };
            }

            var filterContext = new SchemaFilterContext(type, jsonContract, this);
            foreach (var filter in _settings.SchemaFilters)
            {
                filter.Apply(schema, filterContext);
            }

            return schema;
        }

        private Schema CreatePrimitiveSchema(JsonPrimitiveContract primitiveContract)
        {
            var type = primitiveContract.UnderlyingType;

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

        private Schema CreateDictionarySchema(JsonDictionaryContract dictionaryContract, Queue<Type> referencedTypes)
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
                        (name) => CreateSchema(valueType, referencedTypes)
                    )
                };
            }
            else
            {
                return new Schema
                {
                    Type = "object",
                    AdditionalProperties = CreateSchema(valueType, referencedTypes)
                };
            }
        }

        private Schema CreateArraySchema(JsonArrayContract arrayContract, Queue<Type> referencedTypes)
        {
            var itemType = arrayContract.CollectionItemType ?? typeof(object);
            return new Schema
            {
                Type = "array",
                Items = CreateSchema(itemType, referencedTypes)
            };
        }

        private Schema CreateObjectSchema(JsonObjectContract jsonContract, Queue<Type> referencedTypes)
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
                    prop => CreateSchema(prop.PropertyType, referencedTypes).AssignValidationProperties(prop)
                );

            var schema = new Schema
            {
                Required = required.Any() ? required : null, // required can be null but not empty
                Properties = properties,
                AdditionalProperties = hasExtensionData ? new Schema { Type = "object" } : null,
                Type = "object"
            };

            return schema;
        }

        /// <summary>
        /// Unpacks actual type from Nullable-like constructs. When called over
        /// non-nullable type, it just return it back.
        /// </summary>
        private Type GetUnderlyingType(Type type)
        {
            // Option<T>
            if (type.FullNameSansTypeArguments() == "Microsoft.FSharp.Core.FSharpOption`1")
            {
                return type.GetGenericArguments().First();
            }

            // Nullable<T>
            return Nullable.GetUnderlyingType(type) ?? type;
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