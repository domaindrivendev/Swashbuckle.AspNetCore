using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen.SchemaMappings;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaGenerator : ISchemaGenerator
    {
        private readonly SchemaGeneratorOptions _generatorOptions;
        private readonly ISerializerBehavior _serializerBehavior;

        public SchemaGenerator(SchemaGeneratorOptions generatorOptions, ISerializerBehavior serializerBehavior)
        {
            _generatorOptions = generatorOptions;
            _serializerBehavior = serializerBehavior;
        }

        public OpenApiSchema GenerateParameterSchema(ParameterInfo parameterInfo, SchemaRepository schemaRepository) {
            var typeSchema = schemaRepository.GetTypeSchema(parameterInfo.ParameterType);

            if(!TryInlineSchema(typeSchema, out var inlinedSchema))
                return typeSchema;

            ApplyParameterMetadata(inlinedSchema, parameterInfo.ParameterType, parameterInfo);
            return inlinedSchema;
        }

        public OpenApiSchema GenerateMemberSchema(MemberInfo memberInfo, SchemaRepository schemaRepository) {
            var memberType = GetMemberType(memberInfo);
            var typeSchema = schemaRepository.GetTypeSchema(memberType);

            if(!TryInlineSchema(typeSchema, out var inlinedSchema))
                return typeSchema;

            ApplyMemberMetadata(inlinedSchema, memberType, memberInfo);
            return inlinedSchema;
        }

        public static Type GetMemberType(MemberInfo memberInfo) {
            switch(memberInfo) {
                case PropertyInfo property: return property.PropertyType;
                case FieldInfo field: return field.FieldType;
                default: throw new Exception($"Unsupported {nameof(MemberInfo)} type: {memberInfo.GetType()}");
            }
        }

        public SchemaMapping GenerateTypeSchema(Type type)
        {
            if(type.IsNullable(out Type nullableValueType))
                return SchemaMapping.Inline(context => GenerateNullableSchema(type, nullableValueType, context));

            if (type.IsAssignableToOneOf(typeof(IFormFile), typeof(FileResult)))
                return SchemaMapping.Inline(() => new OpenApiSchema { Type = "string", Format = "binary" });

            var dataContract = _serializerBehavior.GetDataContractForType(type);

            switch (dataContract.DataType)
            {
                case DataType.Boolean:
                case DataType.Integer:
                case DataType.Number:
                case DataType.String:
                    return SchemaMapping.ReferenceWhen(
                        type.IsEnum && !_generatorOptions.UseInlineDefinitionsForEnums,
                        () => GeneratePrimitiveSchema(dataContract)
                    );

                case DataType.Array:
                    return SchemaMapping.ReferenceWhen(
                        type == dataContract.ArrayItemType,
                        context => GenerateArraySchema(dataContract, context)
                    );

                case DataType.Dictionary:
                    return SchemaMapping.ReferenceWhen(
                        type == dataContract.DictionaryValueType,
                        context => GenerateDictionarySchema(dataContract, context)
                    );

                case DataType.Object:
                    if (_generatorOptions.UseOneOfForPolymorphism && IsBaseTypeWithKnownSubTypes(type, out IEnumerable<Type> subTypes))
                        return SchemaMapping.Inline(context => GeneratePolymorphicSchema(dataContract, context, subTypes));

                    return SchemaMapping.Reference(context => GenerateObjectSchema(dataContract, context));

                default:
                    return SchemaMapping.Inline(() => new OpenApiSchema());
            }
        }

        private OpenApiSchema GenerateNullableSchema(Type type, Type valueType, ISchemaMappingContext context) {
            var valueTypeSchema = context.SchemaRepository.GetTypeSchema(valueType);

            if(!TryInlineSchema(valueTypeSchema, out var inlinedSchema))
                return valueTypeSchema;

            inlinedSchema.Nullable = true;

            return inlinedSchema;
        }

        private OpenApiSchema GeneratePrimitiveSchema(DataContract dataContract)
        {
            var schema = new OpenApiSchema
            {
                Type = dataContract.DataType.ToString().ToLower(CultureInfo.InvariantCulture),
                Format = dataContract.DataFormat
            };

            if (dataContract.UnderlyingType.IsEnum)
            {
                schema.Enum = dataContract.UnderlyingType.GetEnumValues()
                    .Cast<object>()
                    .Distinct()
                    .Select(value => _serializerBehavior.Serialize(value))
                    .Select(json => OpenApiAnyFactory.CreateFromJson(json))
                    .ToList();
            }

            return schema;
        }

        private OpenApiSchema GenerateArraySchema(DataContract dataContract, ISchemaMappingContext context)
        {
            return new OpenApiSchema
            {
                Type = "array",
                Items = context.SchemaRepository.GetTypeSchema(dataContract.ArrayItemType),
                UniqueItems = dataContract.UnderlyingType.IsSet() ? (bool?)true : null
            };
        }

        private OpenApiSchema GenerateDictionarySchema(DataContract dataContract, ISchemaMappingContext context)
        {
            if (dataContract.DictionaryKeyType.IsEnum)
            {
                // This is a special case where the set of key values is known
                var serializedEnumValues = dataContract.DictionaryKeyType.GetEnumValues()
                    .Cast<object>()
                    .Select(value => _serializerBehavior.Serialize(value));

                var propertyNames = (serializedEnumValues.Any() && serializedEnumValues.First().StartsWith("\""))
                    ? serializedEnumValues.Select(value => value.Replace("\"", string.Empty)) // TODO: address assumption that serializer returns JSON
                    : dataContract.DictionaryKeyType.GetEnumNames();

                return new OpenApiSchema
                {
                    Type = "object",
                    Properties = propertyNames.ToDictionary(name => name, name => context.SchemaRepository.GetTypeSchema(dataContract.DictionaryValueType)),
                    AdditionalPropertiesAllowed = false,
                };
            }
            else
            {
                return new OpenApiSchema
                {
                    Type = "object",
                    AdditionalPropertiesAllowed = true,
                    AdditionalProperties = context.SchemaRepository.GetTypeSchema(dataContract.DictionaryValueType)
                };
            }
        }

        private OpenApiSchema GeneratePolymorphicSchema(DataContract dataContract, ISchemaMappingContext context, IEnumerable<Type> subTypes)
        {
            var schema = new OpenApiSchema
            {
                OneOf = new List<OpenApiSchema>(),
                Discriminator = TryGetDiscriminatorName(dataContract, out string discriminatorName)
                    ? new OpenApiDiscriminator { PropertyName = discriminatorName }
                    : null
            };

            var assignableDataContracts = new[] { dataContract }
                .Concat(subTypes.Select(subType => _serializerBehavior.GetDataContractForType(subType)));

            foreach (var assignableDataContract in assignableDataContracts)
            {
                // Handle the case where the SubTypesSelector returs the base type as a subtype
                // (as is the case with the default selector) by generating the "real" schema for the
                // base type as a supplementary schema.
                var assignableSchema = assignableDataContract.UnderlyingType.Equals(dataContract.UnderlyingType)
                ? context.SchemaRepository.RegisterSupplementarySchema(assignableDataContract.UnderlyingType, ctx => GenerateObjectSchema(assignableDataContract, ctx))
                : context.SchemaRepository.GetTypeSchema(assignableDataContract.UnderlyingType);

                schema.OneOf.Add(assignableSchema);

                if (schema.Discriminator != null && TryGetDiscriminatorValue(assignableDataContract, out string discriminatorValue))
                {
                    schema.Discriminator.Mapping.Add(discriminatorValue, assignableSchema.Reference.ReferenceV3);
                }
            }

            return schema;
        }

        private bool TryGetDiscriminatorName(DataContract dataContract, out string discriminatorName)
        {
            discriminatorName = (_generatorOptions.DiscriminatorNameSelector != null)
                ? _generatorOptions.DiscriminatorNameSelector(dataContract.UnderlyingType)
                : dataContract.ObjectTypeNameProperty;

            return (discriminatorName != null);
        }

        private bool TryGetDiscriminatorValue(DataContract dataContract, out string discriminatorValue)
        {
            discriminatorValue = (_generatorOptions.DiscriminatorValueSelector != null)
                ? _generatorOptions.DiscriminatorValueSelector(dataContract.UnderlyingType)
                : dataContract.ObjectTypeNameValue;

            return (discriminatorValue != null);
        }

        private OpenApiSchema GenerateObjectSchema(DataContract dataContract, ISchemaMappingContext context)
        {
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new SortedSet<string>(),
                AdditionalPropertiesAllowed = false
            };

            // By default, all properties will be defined in this schema
            // However, if "Inheritance" behavior is enabled (see below), this set will be reduced to declared properties only
            var applicableDataProperties = dataContract.ObjectProperties;

            if (_generatorOptions.UseOneOfForPolymorphism || _generatorOptions.UseAllOfForInheritance)
            {
                if (IsBaseTypeWithKnownSubTypes(dataContract.UnderlyingType, out var subTypes))
                {
                    if (_generatorOptions.UseAllOfForInheritance)
                    {
                        // Ensure a schema for all known sub types is generated and added to the repository
                        foreach (var subType in subTypes)
                        {
                            var subTypeContract = _serializerBehavior.GetDataContractForType(subType);
                            // This (arguably incorrectly) bypasses the type mapping/resolution process, but is required
                            // if you want subtypes to explicitly extend the base type.
                            context.SchemaRepository.RegisterSupplementarySchema(subType, ctx => GenerateObjectSchema(subTypeContract, ctx));
                        }
                    }

                    if (_generatorOptions.UseOneOfForPolymorphism
                        && TryGetDiscriminatorName(dataContract, out string discriminatorName))
                    {
                        schema.Properties.Add(discriminatorName, new OpenApiSchema { Type = "string" });
                        schema.Required.Add(discriminatorName);
                    }
                }

                if (IsKnownSubType(dataContract.UnderlyingType, out Type baseType))
                {
                    var baseTypeContract = _serializerBehavior.GetDataContractForType(baseType);

                    if (_generatorOptions.UseAllOfForInheritance)
                    {
                        schema.AllOf = new List<OpenApiSchema>
                        {
                            // This (arguably incorrectly) bypasses the type mapping/resolution process, but is required
                            // if you want subtypes to explicitly extend the base type.
                            context.SchemaRepository.RegisterSupplementarySchema(baseType, ctx => GenerateObjectSchema(dataContract, ctx))
                        };

                        // Reduce the set of properties to be defined in this schema to declared properties only
                        applicableDataProperties = applicableDataProperties
                            .Where(dataProperty => dataProperty.MemberInfo?.DeclaringType == dataContract.UnderlyingType);
                    }

                    if (_generatorOptions.UseOneOfForPolymorphism && !_generatorOptions.UseAllOfForInheritance
                        && TryGetDiscriminatorName(baseTypeContract, out string discriminatorName))
                    {
                        schema.Properties.Add(discriminatorName, new OpenApiSchema { Type = "string" });
                        schema.Required.Add(discriminatorName);
                    }
                }
            }

            foreach (var dataProperty in applicableDataProperties)
            {
                var customAttributes = dataProperty.MemberInfo?.GetInlineAndMetadataAttributes() ?? Enumerable.Empty<object>();

                if (_generatorOptions.IgnoreObsoleteProperties && customAttributes.OfType<ObsoleteAttribute>().Any())
                    continue;

                schema.Properties[dataProperty.Name] = GeneratePropertySchema(dataProperty, context);

                if (dataProperty.IsRequired || customAttributes.OfType<RequiredAttribute>().Any()
                    && !schema.Required.Contains(dataProperty.Name))
                {
                    schema.Required.Add(dataProperty.Name);
                }
            }

            if (dataContract.ObjectExtensionDataType != null)
            {
                schema.AdditionalPropertiesAllowed = true;
                schema.AdditionalProperties = context.SchemaRepository.GetTypeSchema(dataContract.ObjectExtensionDataType);
            }

            return schema;
        }

        // TODO: Why is this different from top-level pipeline for members?
        private OpenApiSchema GeneratePropertySchema(DataProperty dataProperty, ISchemaMappingContext context)
        {
            var schema = dataProperty.MemberInfo != null
            ? context.SchemaRepository.GetMemberSchema(dataProperty.MemberInfo)
            : context.SchemaRepository.GetTypeSchema(dataProperty.MemberType);

            if(!TryInlineSchema(schema, out var inlinedSchema))
                return schema;

            inlinedSchema.Nullable = inlinedSchema.Nullable && dataProperty.IsNullable;
            inlinedSchema.ReadOnly = dataProperty.IsReadOnly;
            inlinedSchema.WriteOnly = dataProperty.IsWriteOnly;

            return inlinedSchema;
        }

        private bool IsBaseTypeWithKnownSubTypes(Type type, out IEnumerable<Type> subTypes)
        {
            subTypes = _generatorOptions.SubTypesSelector(type).Distinct().OrderBy(t => t.FullName).ToList();
            return subTypes.Any(subType => !subType.Equals(type));
        }

        private bool IsKnownSubType(Type type, out Type baseType)
        {
            if ((type.BaseType != null) && (type.BaseType != typeof(object) && _generatorOptions.SubTypesSelector(type.BaseType).Contains(type)))
            {
                baseType = type.BaseType;
                return true;
            }

            baseType = null;
            return false;
        }

        /// <summary>
        /// Attempts to inline the provided schema so that contextual restrictions can be added to the schema.
        /// Per the OpenAPI specification a schema object can be one of either an inline or reference schema;
        /// as such in order to apply restrictions derived from a property/parameter to a reference schema you
        /// MUST reference the schema using one of allOf/anyOf/oneOf.
        /// This method should not exist because schemas generated without UseAllOfToExtendReferences are
        /// fundamentally wrong.
        /// </summary>
        private bool TryInlineSchema(OpenApiSchema schema, out OpenApiSchema inlinedSchema) {
            if(schema.Reference == null) {
                inlinedSchema = schema;
                return true;
            }

            if(_generatorOptions.UseAllOfToExtendReferenceSchemas) {
                inlinedSchema = new OpenApiSchema { AllOf = { schema } };
                return true;
            }

            inlinedSchema = null;
            return false;
        }

        private void ApplyMemberMetadata(OpenApiSchema schema, Type type, MemberInfo memberInfo)
        {
            if(schema.Reference != null)
                throw new ArgumentException($"Provided {nameof(schema)} is a reference schema.", nameof(schema));

            var customAttributes = memberInfo.GetInlineAndMetadataAttributes();

            schema.Nullable = type.IsReferenceOrNullableType();

            var defaultValueAttribute = customAttributes.OfType<DefaultValueAttribute>().FirstOrDefault();
            if (defaultValueAttribute != null)
            {
                var defaultAsJson = _serializerBehavior.Serialize(defaultValueAttribute.Value);
                schema.Default = OpenApiAnyFactory.CreateFromJson(defaultAsJson); // TODO: address assumption that serializer returns JSON
            }

            var obsoleteAttribute = customAttributes.OfType<ObsoleteAttribute>().FirstOrDefault();
            if (obsoleteAttribute != null)
            {
                schema.Deprecated = true;
            }

            schema.ApplyValidationAttributes(customAttributes);
        }

        private void ApplyParameterMetadata(OpenApiSchema schema, Type type, ParameterInfo parameterInfo)
        {
            if(schema.Reference != null)
                throw new ArgumentException($"Provided {nameof(schema)} is a reference schema.", nameof(schema));

            schema.Nullable = type.IsReferenceOrNullableType();

            if (parameterInfo.HasDefaultValue)
            {
                var defaultAsJson = _serializerBehavior.Serialize(parameterInfo.DefaultValue);
                schema.Default = OpenApiAnyFactory.CreateFromJson(defaultAsJson);
            }

            schema.ApplyValidationAttributes(parameterInfo.GetCustomAttributes());
        }
    }
}
