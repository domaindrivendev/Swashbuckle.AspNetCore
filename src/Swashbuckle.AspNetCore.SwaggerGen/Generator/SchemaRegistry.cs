using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaRegistry : ISchemaRegistry
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly IContractResolver _jsonContractResolver;
        private readonly SchemaRegistryOptions _options;
        private readonly SchemaIdManager _schemaIdManager;

        public SchemaRegistry(
            JsonSerializerSettings jsonSerializerSettings,
            SchemaRegistryOptions options = null)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _jsonContractResolver = _jsonSerializerSettings.ContractResolver ?? new DefaultContractResolver();
            _options = options ?? new SchemaRegistryOptions();
            _schemaIdManager = new SchemaIdManager(_options.SchemaIdSelector);
            Schemas = new Dictionary<string, OpenApiSchema>();
        }

        public IDictionary<string, OpenApiSchema> Schemas { get; private set; }

        public OpenApiSchema GetOrRegister(Type type)
        {
            var referencedTypes = new Queue<Type>();
            var openApiSchema = CreateSchema(type, referencedTypes);

            // Ensure all referenced types have a corresponding definition
            while (referencedTypes.Any())
            {
                var referencedType = referencedTypes.Dequeue();
                var OpenApiSchemaId = _schemaIdManager.IdFor(referencedType);
                if (Schemas.ContainsKey(OpenApiSchemaId)) continue;

                // NOTE: Add the OpenApiSchemaId first with a null value. This indicates a work-in-progress
                // and prevents a stack overflow by ensuring the above condition is met if the same
                // type ends up back on the referencedTypes queue via recursion within 'CreateInlineOpenApiSchema'
                Schemas.Add(OpenApiSchemaId, null);
                Schemas[OpenApiSchemaId] = CreateInlineSchema(referencedType, referencedTypes);
            }

            return openApiSchema;
        }

        private OpenApiSchema CreateSchema(Type type, Queue<Type> referencedTypes)
        {
            // If Option<T> (F#), use the type argument
            if (type.IsFSharpOption())
                type = type.GetGenericArguments()[0];

            // Special handling for form binding types
            if (typeof(IFormFile).IsAssignableFrom(type))
            {
                return new OpenApiSchema { Type = "string", Format = "binary" };
            }

            var jsonContract = _jsonContractResolver.ResolveContract(type);

            var createReference = !_options.CustomTypeMappings.ContainsKey(type)
                && type != typeof(object)
                && (// Type describes an object
                    jsonContract is JsonObjectContract ||
                    // Type is self-referencing
                    jsonContract.IsSelfReferencingArrayOrDictionary() ||
                    // Type is enum and opt-in flag set
                    (type.GetTypeInfo().IsEnum && _options.UseReferencedDefinitionsForEnums));

            return createReference
                ? CreateReferenceSchema(type, referencedTypes)
                : CreateInlineSchema(type, referencedTypes);
        }

        private OpenApiSchema CreateReferenceSchema(Type type, Queue<Type> referencedTypes)
        {
            referencedTypes.Enqueue(type);
            return new OpenApiSchema
            {
                Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = _schemaIdManager.IdFor(type) }
            };
        }

        private OpenApiSchema CreateInlineSchema(Type type, Queue<Type> referencedTypes)
        {
            OpenApiSchema OpenApiSchema;

            var jsonContract = _jsonContractResolver.ResolveContract(type);

            if (_options.CustomTypeMappings.ContainsKey(type))
            {
                OpenApiSchema = _options.CustomTypeMappings[type]();
            }
            else
            {
                // TODO: Perhaps a "Chain of Responsibility" would clean this up a little?
                if (jsonContract is JsonPrimitiveContract)
                    OpenApiSchema = CreatePrimitiveSchema((JsonPrimitiveContract)jsonContract);
                else if (jsonContract is JsonDictionaryContract)
                    OpenApiSchema = CreateDictionarySchema((JsonDictionaryContract)jsonContract, referencedTypes);
                else if (jsonContract is JsonArrayContract)
                    OpenApiSchema = CreateArraySchema((JsonArrayContract)jsonContract, referencedTypes);
                else if (jsonContract is JsonObjectContract && type != typeof(object))
                    OpenApiSchema = CreateObjectSchema((JsonObjectContract)jsonContract, referencedTypes);
                else
                    // None of the above, fallback to abstract "object"
                    OpenApiSchema = new OpenApiSchema { Type = "object" };
            }

            var filterContext = new SchemaFilterContext(type, jsonContract, this);
            foreach (var filter in _options.SchemaFilters)
            {
                filter.Apply(OpenApiSchema, filterContext);
            }

            return OpenApiSchema;
        }

        private OpenApiSchema CreatePrimitiveSchema(JsonPrimitiveContract primitiveContract)
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
            return new OpenApiSchema { Type = "string" };
        }

        private OpenApiSchema CreateEnumSchema(JsonPrimitiveContract primitiveContract, Type type)
        {
            var stringEnumConverter = primitiveContract.Converter as StringEnumConverter
                ?? _jsonSerializerSettings.Converters.OfType<StringEnumConverter>().FirstOrDefault();

            if (_options.DescribeAllEnumsAsStrings || stringEnumConverter != null)
            {
                var camelCase = _options.DescribeStringEnumsInCamelCase
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

                return new OpenApiSchema
                {
                    Type = "string",
                    Enum = enumNames
                        .Select(name => new OpenApiString(name))
                        .ToList<IOpenApiAny>()
                };
            }

            return new OpenApiSchema
            {
                Type = "integer",
                Format = "int32",
                Enum = Enum.GetValues(type).Cast<int>()
                    .Select(value => new OpenApiInteger(value))
                    .ToList<IOpenApiAny>()
            };
        }

        private OpenApiSchema CreateDictionarySchema(JsonDictionaryContract dictionaryContract, Queue<Type> referencedTypes)
        {
            var keyType = dictionaryContract.DictionaryKeyType ?? typeof(object);
            var valueType = dictionaryContract.DictionaryValueType ?? typeof(object);

            if (keyType.GetTypeInfo().IsEnum)
            {
                return new OpenApiSchema
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
                return new OpenApiSchema
                {
                    Type = "object",
                    AdditionalProperties = CreateSchema(valueType, referencedTypes)
                };
            }
        }

        private OpenApiSchema CreateArraySchema(JsonArrayContract arrayContract, Queue<Type> referencedTypes)
        {
            var type = arrayContract.UnderlyingType;
            var itemType = arrayContract.CollectionItemType ?? typeof(object);

            var isASet = (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(ISet<>)
                || type.GetInterfaces().Any(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>)));

            return new OpenApiSchema
            {
                Type = "array",
                Items = CreateSchema(itemType, referencedTypes),
                UniqueItems = isASet
            };
        }

        private OpenApiSchema CreateObjectSchema(JsonObjectContract jsonContract, Queue<Type> referencedTypes)
        {
            var applicableJsonProperties = jsonContract.Properties
                .Where(prop => !prop.Ignored)
                .Where(prop => !(_options.IgnoreObsoleteProperties && prop.IsObsolete()))
                .Select(prop => prop);

            var required = applicableJsonProperties
                .Where(prop => prop.IsRequired())
                .Select(propInfo => propInfo.PropertyName)
                .ToList();

            var hasExtensionData = jsonContract.ExtensionDataValueType != null;

            var properties = applicableJsonProperties
                .ToDictionary(
                    prop => prop.PropertyName,
                    prop => CreatePropertyOpenApiSchema(prop, referencedTypes));

            var OpenApiSchema = new OpenApiSchema
            {
                Required = new SortedSet<string>(required),
                Properties = properties,
                AdditionalProperties = hasExtensionData ? new OpenApiSchema { Type = "object" } : null,
                Type = "object",
            };

            return OpenApiSchema;
        }

        private OpenApiSchema CreatePropertyOpenApiSchema(JsonProperty jsonProperty, Queue<Type> referencedTypes)
        {
            var OpenApiSchema = CreateSchema(jsonProperty.PropertyType, referencedTypes);

            if (!jsonProperty.Writable)
                OpenApiSchema.ReadOnly = true;

            if (!jsonProperty.Readable)
                OpenApiSchema.WriteOnly = true;

            if (jsonProperty.TryGetMemberInfo(out MemberInfo memberInfo))
                OpenApiSchema.AssignAttributeMetadata(memberInfo.GetCustomAttributes(true));

            return OpenApiSchema;
        }

        private static readonly Dictionary<Type, Func<OpenApiSchema>> PrimitiveTypeMap = new Dictionary<Type, Func<OpenApiSchema>>
        {
            { typeof(short), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(ushort), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(int), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(uint), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(long), () => new OpenApiSchema { Type = "integer", Format = "int64" } },
            { typeof(ulong), () => new OpenApiSchema { Type = "integer", Format = "int64" } },
            { typeof(float), () => new OpenApiSchema { Type = "number", Format = "float" } },
            { typeof(double), () => new OpenApiSchema { Type = "number", Format = "double" } },
            { typeof(decimal), () => new OpenApiSchema { Type = "number", Format = "double" } },
            { typeof(byte), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(sbyte), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(byte[]), () => new OpenApiSchema { Type = "string", Format = "byte" } },
            { typeof(sbyte[]), () => new OpenApiSchema { Type = "string", Format = "byte" } },
            { typeof(bool), () => new OpenApiSchema { Type = "boolean" } },
            { typeof(DateTime), () => new OpenApiSchema { Type = "string", Format = "date-time" } },
            { typeof(DateTimeOffset), () => new OpenApiSchema { Type = "string", Format = "date-time" } },
            { typeof(Guid), () => new OpenApiSchema { Type = "string", Format = "uuid" } }
        };
    }
}