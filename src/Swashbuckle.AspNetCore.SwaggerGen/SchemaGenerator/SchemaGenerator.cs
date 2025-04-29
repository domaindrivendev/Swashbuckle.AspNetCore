using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class SchemaGenerator(
    SchemaGeneratorOptions generatorOptions,
    ISerializerDataContractResolver serializerDataContractResolver) : ISchemaGenerator
{
    private readonly SchemaGeneratorOptions _generatorOptions = generatorOptions;
    private readonly ISerializerDataContractResolver _serializerDataContractResolver = serializerDataContractResolver;

    [Obsolete($"{nameof(IOptions<MvcOptions>)} is no longer used. This constructor will be removed in a future major release.")]
    public SchemaGenerator(
        SchemaGeneratorOptions generatorOptions,
        ISerializerDataContractResolver serializerDataContractResolver,
        IOptions<MvcOptions> mvcOptions)
        : this(generatorOptions, serializerDataContractResolver)
    {
    }

    public IOpenApiSchema GenerateSchema(
        Type modelType,
        SchemaRepository schemaRepository,
        MemberInfo memberInfo = null,
        ParameterInfo parameterInfo = null,
        ApiParameterRouteInfo routeInfo = null)
    {
        if (memberInfo != null)
        {
            return GenerateSchemaForMember(modelType, schemaRepository, memberInfo);
        }
        else if (parameterInfo != null)
        {
            return GenerateSchemaForParameter(modelType, schemaRepository, parameterInfo, routeInfo);
        }
        else
        {
            return GenerateSchemaForType(modelType, schemaRepository);
        }
    }

    private IOpenApiSchema GenerateSchemaForMember(
        Type modelType,
        SchemaRepository schemaRepository,
        MemberInfo memberInfo,
        DataProperty dataProperty = null)
    {
        var dataContract = GetDataContractFor(modelType);

        var schema = _generatorOptions.UseOneOfForPolymorphism && IsBaseTypeWithKnownTypesDefined(dataContract, out var knownTypesDataContracts)
            ? GeneratePolymorphicSchema(schemaRepository, knownTypesDataContracts)
            : GenerateConcreteSchema(dataContract, schemaRepository);

        if (_generatorOptions.UseAllOfToExtendReferenceSchemas && schema is OpenApiSchemaReference reference)
        {
            schema = new OpenApiSchema()
            {
                AllOf = [reference],
            };
        }

        if (schema is OpenApiSchema concrete)
        {
            var customAttributes = memberInfo.GetInlineAndMetadataAttributes();

            // Nullable, ReadOnly & WriteOnly are only relevant for Schema "properties" (i.e. where dataProperty is non-null)
            if (dataProperty != null)
            {
                var requiredAttribute = customAttributes.OfType<RequiredAttribute>().FirstOrDefault();

                var nullable = _generatorOptions.SupportNonNullableReferenceTypes
                    ? dataProperty.IsNullable && requiredAttribute == null && !memberInfo.IsNonNullableReferenceType()
                    : dataProperty.IsNullable && requiredAttribute == null;

                if (nullable)
                {
                    concrete.Type |= JsonSchemaType.Null;
                }
                else
                {
                    concrete.Type &= ~JsonSchemaType.Null;
                }

                concrete.ReadOnly = dataProperty.IsReadOnly;
                concrete.WriteOnly = dataProperty.IsWriteOnly;
                concrete.MinLength = modelType == typeof(string) && requiredAttribute is { AllowEmptyStrings: false } ? 1 : null;
            }

            var defaultValueAttribute = customAttributes.OfType<DefaultValueAttribute>().FirstOrDefault();
            if (defaultValueAttribute != null)
            {
                concrete.Default = GenerateDefaultValue(dataContract, modelType, defaultValueAttribute.Value);
            }

            var obsoleteAttribute = customAttributes.OfType<ObsoleteAttribute>().FirstOrDefault();
            if (obsoleteAttribute != null)
            {
                concrete.Deprecated = true;
            }

            // NullableAttribute behaves differently for Dictionaries
            if (schema.AdditionalPropertiesAllowed && modelType.IsGenericType)
            {
                var genericTypes = modelType
                    .GetInterfaces()
#if !NET
                    .Concat([modelType])
#else
                    .Append(modelType)
#endif
                    .Where(t => t.IsGenericType)
                    .ToArray();

                var isDictionaryType =
                    genericTypes.Any(t => t.GetGenericTypeDefinition() == typeof(IDictionary<,>)) ||
                    genericTypes.Any(t => t.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>));

                if (isDictionaryType && schema.AdditionalProperties is OpenApiSchema additionalProperties)
                {
                    if (!memberInfo.IsDictionaryValueNonNullable())
                    {
                        additionalProperties.Type |= JsonSchemaType.Null;
                    }
                    else
                    {
                        additionalProperties.Type &= ~JsonSchemaType.Null;
                    }
                }
            }

            schema.ApplyValidationAttributes(customAttributes);

            ApplyFilters(schema, modelType, schemaRepository, memberInfo: memberInfo);
        }

        return schema;
    }

    private IOpenApiSchema GenerateSchemaForParameter(
        Type modelType,
        SchemaRepository schemaRepository,
        ParameterInfo parameterInfo,
        ApiParameterRouteInfo routeInfo)
    {
        var dataContract = GetDataContractFor(modelType);

        var schema = _generatorOptions.UseOneOfForPolymorphism && IsBaseTypeWithKnownTypesDefined(dataContract, out var knownTypesDataContracts)
            ? GeneratePolymorphicSchema(schemaRepository, knownTypesDataContracts)
            : GenerateConcreteSchema(dataContract, schemaRepository);

        if (_generatorOptions.UseAllOfToExtendReferenceSchemas && schema is OpenApiSchemaReference reference)
        {
            schema = new OpenApiSchema() { AllOf = [reference] };
        }

        if (schema is OpenApiSchema concrete)
        {
            var customAttributes = parameterInfo.GetCustomAttributes();

            var defaultValue = parameterInfo.HasDefaultValue
                ? parameterInfo.DefaultValue
                : customAttributes.OfType<DefaultValueAttribute>().FirstOrDefault()?.Value;

            if (defaultValue != null)
            {
                concrete.Default = GenerateDefaultValue(dataContract, modelType, defaultValue);
            }

            schema.ApplyValidationAttributes(customAttributes);
            if (routeInfo != null)
            {
                schema.ApplyRouteConstraints(routeInfo);
            }

            ApplyFilters(schema, modelType, schemaRepository, parameterInfo: parameterInfo);
        }

        return schema;
    }

    private IOpenApiSchema GenerateSchemaForType(Type modelType, SchemaRepository schemaRepository)
    {
        var dataContract = GetDataContractFor(modelType);

        var schema = _generatorOptions.UseOneOfForPolymorphism && IsBaseTypeWithKnownTypesDefined(dataContract, out var knownTypesDataContracts)
            ? GeneratePolymorphicSchema(schemaRepository, knownTypesDataContracts)
            : GenerateConcreteSchema(dataContract, schemaRepository);

        if (schema is not OpenApiSchemaReference)
        {
            ApplyFilters(schema, modelType, schemaRepository);
            if (Nullable.GetUnderlyingType(modelType) != null && schema is OpenApiSchema concrete)
            {
                concrete.Type |= JsonSchemaType.Null;
            }
        }

        return schema;
    }

    private DataContract GetDataContractFor(Type modelType)
    {
        return _serializerDataContractResolver.GetDataContractForType(modelType);
    }

    private bool IsBaseTypeWithKnownTypesDefined(DataContract dataContract, out IEnumerable<DataContract> knownTypesDataContracts)
    {
        knownTypesDataContracts = null;

        if (dataContract.DataType != DataType.Object)
        {
            return false;
        }

        var subTypes = _generatorOptions.SubTypesSelector(dataContract.UnderlyingType);

        if (!subTypes.Any())
        {
            return false;
        }

        var knownTypes = !dataContract.UnderlyingType.IsAbstract
            ? new[] { dataContract.UnderlyingType }.Union(subTypes)
            : subTypes;

        knownTypesDataContracts = knownTypes.Select(GetDataContractFor);
        return true;
    }

    private OpenApiSchema GeneratePolymorphicSchema(
        SchemaRepository schemaRepository,
        IEnumerable<DataContract> knownTypesDataContracts)
    {
        return new OpenApiSchema
        {
            OneOf = [.. knownTypesDataContracts.Select(allowedTypeDataContract => GenerateConcreteSchema(allowedTypeDataContract, schemaRepository))]
        };
    }

    private static readonly Type[] BinaryStringTypes =
    [
        typeof(IFormFile),
        typeof(FileResult),
        typeof(Stream),
#if NET
        typeof(System.IO.Pipelines.PipeReader),
#endif
    ];

    private IOpenApiSchema GenerateConcreteSchema(DataContract dataContract, SchemaRepository schemaRepository)
    {
        if (TryGetCustomTypeMapping(dataContract.UnderlyingType, out Func<IOpenApiSchema> customSchemaFactory))
        {
            return customSchemaFactory();
        }

        if (dataContract.UnderlyingType.IsAssignableToOneOf(BinaryStringTypes))
        {
            return new OpenApiSchema { Type = JsonSchemaTypes.String, Format = "binary" };
        }

        Func<OpenApiSchema> schemaFactory;
        bool returnAsReference;

        switch (dataContract.DataType)
        {
            case DataType.Boolean:
            case DataType.Integer:
            case DataType.Number:
            case DataType.String:
                {
                    schemaFactory = () => CreatePrimitiveSchema(dataContract);
                    returnAsReference = dataContract.UnderlyingType.IsEnum && !_generatorOptions.UseInlineDefinitionsForEnums;
                    break;
                }

            case DataType.Array:
                {
                    schemaFactory = () => CreateArraySchema(dataContract, schemaRepository);
                    returnAsReference = dataContract.UnderlyingType == dataContract.ArrayItemType;
                    break;
                }

            case DataType.Dictionary:
                {
                    schemaFactory = () => CreateDictionarySchema(dataContract, schemaRepository);
                    returnAsReference = dataContract.UnderlyingType == dataContract.DictionaryValueType;
                    break;
                }

            case DataType.Object:
                {
                    schemaFactory = () => CreateObjectSchema(dataContract, schemaRepository);
                    returnAsReference = true;
                    break;
                }

            default:
                {
                    schemaFactory = () => new OpenApiSchema();
                    returnAsReference = false;
                    break;
                }
        }

        return returnAsReference
            ? GenerateReferencedSchema(dataContract, schemaRepository, schemaFactory)
            : schemaFactory();
    }

    private bool TryGetCustomTypeMapping(Type modelType, out Func<IOpenApiSchema> schemaFactory)
    {
        return
            _generatorOptions.CustomTypeMappings.TryGetValue(modelType, out schemaFactory) ||
            (modelType.IsConstructedGenericType && _generatorOptions.CustomTypeMappings.TryGetValue(modelType.GetGenericTypeDefinition(), out schemaFactory));
    }

    private static OpenApiSchema CreatePrimitiveSchema(DataContract dataContract)
    {
        var schema = new OpenApiSchema
        {
            Type = FromDataType(dataContract.DataType),
            Format = dataContract.DataFormat
        };

#pragma warning disable CS0618 // Type or member is obsolete
        // For backwards compatibility only - EnumValues is obsolete
        if (dataContract.EnumValues != null)
        {
            schema.Enum = [.. dataContract.EnumValues
                .Select(value => JsonSerializer.Serialize(value))
                .Distinct()
                .Select(JsonModelFactory.CreateFromJson)];

            return schema;
        }
#pragma warning restore CS0618 // Type or member is obsolete

        if (dataContract.UnderlyingType.IsEnum)
        {
            schema.Enum = [.. dataContract.UnderlyingType.GetEnumValues()
                .Cast<object>()
                .Select(value => dataContract.JsonConverter(value))
                .Distinct()
                .Select(JsonModelFactory.CreateFromJson)];
        }

        return schema;
    }

    private OpenApiSchema CreateArraySchema(DataContract dataContract, SchemaRepository schemaRepository)
    {
        var hasUniqueItems =
            dataContract.UnderlyingType.IsConstructedFrom(typeof(ISet<>), out _) ||
            dataContract.UnderlyingType.IsConstructedFrom(typeof(KeyedCollection<,>), out _);

        return new OpenApiSchema
        {
            Type = JsonSchemaTypes.Array,
            Items = GenerateSchema(dataContract.ArrayItemType, schemaRepository),
            UniqueItems = hasUniqueItems ? true : null
        };
    }

    private OpenApiSchema CreateDictionarySchema(DataContract dataContract, SchemaRepository schemaRepository)
    {
        var knownKeysProperties = dataContract.DictionaryKeys?.ToDictionary(
            name => name,
            _ => GenerateSchema(dataContract.DictionaryValueType, schemaRepository));

        if (knownKeysProperties?.Count > 0)
        {
            // This is a special case where the set of key values is known (e.g. if the key type is an enum)
            return new OpenApiSchema
            {
                Type = JsonSchemaTypes.Object,
                Properties = knownKeysProperties,
                AdditionalPropertiesAllowed = false
            };
        }

        return new OpenApiSchema
        {
            Type = JsonSchemaTypes.Object,
            AdditionalPropertiesAllowed = true,
            AdditionalProperties = GenerateSchema(dataContract.DictionaryValueType, schemaRepository)
        };
    }

    private OpenApiSchema CreateObjectSchema(DataContract dataContract, SchemaRepository schemaRepository)
    {
        var schema = new OpenApiSchema
        {
            Type = JsonSchemaTypes.Object,
            Properties = [],
            Required = [],
            AdditionalPropertiesAllowed = false
        };

        OpenApiSchema root = schema;
        var applicableDataProperties = dataContract.ObjectProperties;

        if (_generatorOptions.UseAllOfForInheritance || _generatorOptions.UseOneOfForPolymorphism)
        {
            if (IsKnownSubType(dataContract, out var baseTypeDataContract))
            {
                var baseTypeSchema = GenerateConcreteSchema(baseTypeDataContract, schemaRepository);

                if (_generatorOptions.UseAllOfForInheritance)
                {
                    root = new OpenApiSchema();
                    root.AllOf ??= [];
                    root.AllOf.Add(baseTypeSchema);
                }
                else
                {
                    schema.AllOf ??= [];
                    schema.AllOf.Add(baseTypeSchema);
                }

                applicableDataProperties = applicableDataProperties
                    .Where(dataProperty => dataProperty.MemberInfo.DeclaringType == dataContract.UnderlyingType);
            }

            if (IsBaseTypeWithKnownTypesDefined(dataContract, out var knownTypesDataContracts))
            {
                foreach (var knownTypeDataContract in knownTypesDataContracts)
                {
                    // Ensure schema is generated for all known types
                    GenerateConcreteSchema(knownTypeDataContract, schemaRepository);
                }

                if (TryGetDiscriminatorFor(dataContract, schemaRepository, knownTypesDataContracts, out var discriminator))
                {
                    schema.Properties.Add(discriminator.PropertyName, new OpenApiSchema { Type = JsonSchemaTypes.String });
                    schema.Required.Add(discriminator.PropertyName);
                    schema.Discriminator = discriminator;
                }
            }
        }

        foreach (var dataProperty in applicableDataProperties)
        {
            var customAttributes = dataProperty.MemberInfo?.GetInlineAndMetadataAttributes() ?? [];

            if (_generatorOptions.IgnoreObsoleteProperties && customAttributes.OfType<ObsoleteAttribute>().Any())
            {
                continue;
            }

            schema.Properties[dataProperty.Name] = (dataProperty.MemberInfo != null)
                ? GenerateSchemaForMember(dataProperty.MemberType, schemaRepository, dataProperty.MemberInfo, dataProperty)
                : GenerateSchemaForType(dataProperty.MemberType, schemaRepository);

            var markNonNullableTypeAsRequired =
                _generatorOptions.NonNullableReferenceTypesAsRequired &&
                (dataProperty.MemberInfo?.IsNonNullableReferenceType() ?? false);

            if ((
                dataProperty.IsRequired
                || markNonNullableTypeAsRequired
                || customAttributes.OfType<RequiredAttribute>().Any()
#if NET
                || customAttributes.OfType<System.Runtime.CompilerServices.RequiredMemberAttribute>().Any()
#endif
                )
                && !schema.Required.Contains(dataProperty.Name))
            {
                schema.Required.Add(dataProperty.Name);
            }
        }

        if (dataContract.ObjectExtensionDataType != null)
        {
            schema.AdditionalPropertiesAllowed = true;
            schema.AdditionalProperties = GenerateSchema(dataContract.ObjectExtensionDataType, schemaRepository);
        }

        if (root != schema)
        {
            root.AllOf.Add(schema);
        }

        if (schema.Required?.Count > 1)
        {
            schema.Required = [.. new SortedSet<string>(schema.Required)];
        }

        return root;
    }

    private bool IsKnownSubType(DataContract dataContract, out DataContract baseTypeDataContract)
    {
        baseTypeDataContract = null;

        var baseType = dataContract.UnderlyingType.BaseType;
        while (baseType != null && baseType != typeof(object))
        {
            if (_generatorOptions.SubTypesSelector(baseType).Contains(dataContract.UnderlyingType))
            {
                baseTypeDataContract = GetDataContractFor(baseType);
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    private bool TryGetDiscriminatorFor(
        DataContract dataContract,
        SchemaRepository schemaRepository,
        IEnumerable<DataContract> knownTypesDataContracts,
        out OpenApiDiscriminator discriminator)
    {
        discriminator = null;

        var discriminatorName = _generatorOptions.DiscriminatorNameSelector(dataContract.UnderlyingType)
            ?? dataContract.ObjectTypeNameProperty;

        if (discriminatorName == null) return false;

        discriminator = new OpenApiDiscriminator
        {
            PropertyName = discriminatorName
        };

        foreach (var knownTypeDataContract in knownTypesDataContracts)
        {
            var discriminatorValue =
                _generatorOptions.DiscriminatorValueSelector(knownTypeDataContract.UnderlyingType)
                ?? knownTypeDataContract.ObjectTypeNameValue;

            if (discriminatorValue != null)
            {
                if (GenerateConcreteSchema(knownTypeDataContract, schemaRepository) is OpenApiSchemaReference reference)
                {
                    discriminator.Mapping ??= [];
                    discriminator.Mapping.Add(discriminatorValue, reference);
                }
            }
        }

        return true;
    }

    private OpenApiSchemaReference GenerateReferencedSchema(
        DataContract dataContract,
        SchemaRepository schemaRepository,
        Func<OpenApiSchema> definitionFactory)
    {
        if (schemaRepository.TryLookupByType(dataContract.UnderlyingType, out var referenceSchema))
        {
            return referenceSchema;
        }

        var schemaId = _generatorOptions.SchemaIdSelector(dataContract.UnderlyingType);

        schemaRepository.RegisterType(dataContract.UnderlyingType, schemaId);

        var schema = definitionFactory();

        ApplyFilters(schema, dataContract.UnderlyingType, schemaRepository);

        return schemaRepository.AddDefinition(schemaId, schema);
    }

    private void ApplyFilters(
        IOpenApiSchema schema,
        Type type,
        SchemaRepository schemaRepository,
        MemberInfo memberInfo = null,
        ParameterInfo parameterInfo = null)
    {
        var filterContext = new SchemaFilterContext(
            type: type,
            schemaGenerator: this,
            schemaRepository: schemaRepository,
            memberInfo: memberInfo,
            parameterInfo: parameterInfo);

        foreach (var filter in _generatorOptions.SchemaFilters)
        {
            filter.Apply(schema, filterContext);
        }
    }

    private System.Text.Json.Nodes.JsonNode GenerateDefaultValue(
        DataContract dataContract,
        Type modelType,
        object defaultValue)
    {
        // If the types do not match (e.g. a default which is an integer is specified for a double),
        // attempt to coerce the default value to the correct type so that it can be serialized correctly.
        // See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2885 and
        // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2904.
        var defaultValueType = defaultValue?.GetType();

        if (defaultValueType != null && defaultValueType != modelType)
        {
            dataContract = GetDataContractFor(defaultValueType);
        }

        var defaultAsJson = dataContract.JsonConverter(defaultValue);
        return JsonModelFactory.CreateFromJson(defaultAsJson);
    }

    private static JsonSchemaType FromDataType(DataType dataType) =>
#if NET
       Enum.Parse<JsonSchemaType>(dataType.ToString());
#else
       (JsonSchemaType)Enum.Parse(typeof(JsonSchemaType), dataType.ToString());
#endif
}
