using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaGenerator : ISchemaGenerator
    {
        private readonly SchemaGeneratorOptions _generatorOptions;
        private readonly ISerializerDataContractResolver _serializerDataContractResolver;

        public SchemaGenerator(SchemaGeneratorOptions generatorOptions, ISerializerDataContractResolver serializerDataContractResolver)
        {
            _generatorOptions = generatorOptions;
            _serializerDataContractResolver = serializerDataContractResolver;
        }

        public OpenApiSchema GenerateSchema(
            Type type,
            SchemaRepository schemaRepository,
            MemberInfo memberInfo = null,
            ParameterInfo parameterInfo = null)
        {
            var dataContract = GetDataContractFor(type);

            var schema = _generatorOptions.UseOneOfForPolymorphism && IsBaseTypeWithKnownTypesDefined(dataContract, out var knownTypesDataContracts)
                ? GeneratePolymorphicSchema(dataContract, schemaRepository, knownTypesDataContracts)
                : GenerateConcreteSchema(dataContract, schemaRepository);

            if (memberInfo != null)
            {
                ApplyMemberMetadata(schema, type, memberInfo, dataContract);
            }
            else if (parameterInfo != null)
            {
                ApplyParameterMetadata(schema, type, parameterInfo, dataContract);
            }

            if (schema.Reference == null)
            {
                ApplyFilters(schema, type, schemaRepository, memberInfo, parameterInfo);
            }

            return schema;
        }

        private DataContract GetDataContractFor(Type type)
        {
            var effectiveType = Nullable.GetUnderlyingType(type) ?? type;
            return _serializerDataContractResolver.GetDataContractForType(effectiveType);
        }

        private bool IsBaseTypeWithKnownTypesDefined(DataContract dataContract, out IEnumerable<DataContract> knownTypesDataContracts)
        {
            knownTypesDataContracts = null;

            if (dataContract.DataType != DataType.Object) return false;

            var subTypes = _generatorOptions.SubTypesSelector(dataContract.UnderlyingType);

            if (!subTypes.Any()) return false;

            var knownTypes = !dataContract.UnderlyingType.IsAbstract
                ? new[] { dataContract.UnderlyingType }.Union(subTypes)
                : subTypes;

            knownTypesDataContracts = knownTypes.Select(knownType => GetDataContractFor(knownType));
            return true;
        }

        private OpenApiSchema GeneratePolymorphicSchema(
            DataContract dataContract,
            SchemaRepository schemaRepository,
            IEnumerable<DataContract> knownTypesDataContracts)
        {
            return new OpenApiSchema
            {
                OneOf = knownTypesDataContracts
                    .Select(allowedTypeDataContract => GenerateConcreteSchema(allowedTypeDataContract, schemaRepository))
                    .ToList()
            };
        }

        private OpenApiSchema GenerateConcreteSchema(DataContract dataContract, SchemaRepository schemaRepository)
        {
            if (TryGetCustomTypeMapping(dataContract.UnderlyingType, out Func<OpenApiSchema> customSchemaFactory))
            {
                return customSchemaFactory();
            }

            if (dataContract.UnderlyingType.IsAssignableToOneOf(typeof(IFormFile), typeof(FileResult)))
            {
                return new OpenApiSchema { Type = "string", Format = "binary" };
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

        private bool TryGetCustomTypeMapping(Type type, out Func<OpenApiSchema> schemaFactory)
        {
            return _generatorOptions.CustomTypeMappings.TryGetValue(type, out schemaFactory)
                || (type.IsConstructedGenericType && _generatorOptions.CustomTypeMappings.TryGetValue(type.GetGenericTypeDefinition(), out schemaFactory));
        }

        private OpenApiSchema CreatePrimitiveSchema(DataContract dataContract)
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
                    .Select(value => dataContract.JsonConverter(value))
                    .Select(valueAsJson => OpenApiAnyFactory.CreateFromJson(valueAsJson))
                    .ToList();
            }

            return schema;
        }

        private OpenApiSchema CreateArraySchema(DataContract dataContract, SchemaRepository schemaRepository)
        {
            var hasUniqueItems = dataContract.UnderlyingType.IsConstructedFrom(typeof(ISet<>), out _)
                || dataContract.UnderlyingType.IsConstructedFrom(typeof(KeyedCollection<,>), out _);

            return new OpenApiSchema
            {
                Type = "array",
                Items = GenerateSchema(dataContract.ArrayItemType, schemaRepository),
                UniqueItems = hasUniqueItems ? (bool?)true : null
            };
        }

        private OpenApiSchema CreateDictionarySchema(DataContract dataContract, SchemaRepository schemaRepository)
        {
            if (dataContract.DictionaryKeys != null)
            {
                // This is a special case where the set of key values is known (e.g. if the key type is an enum)
                return new OpenApiSchema
                {
                    Type = "object",
                    Properties = dataContract.DictionaryKeys.ToDictionary(
                        name => name,
                        name => GenerateSchema(dataContract.DictionaryValueType, schemaRepository)),
                    AdditionalPropertiesAllowed = false,
                };
            }
            else
            {
                return new OpenApiSchema
                {
                    Type = "object",
                    AdditionalPropertiesAllowed = true,
                    AdditionalProperties = GenerateSchema(dataContract.DictionaryValueType, schemaRepository)
                };
            }
        }

        private OpenApiSchema CreateObjectSchema(DataContract dataContract, SchemaRepository schemaRepository)
        {
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new SortedSet<string>(),
                AdditionalPropertiesAllowed = false
            };

            var applicableDataProperties = dataContract.ObjectProperties;

            if (_generatorOptions.UseAllOfForInheritance || _generatorOptions.UseOneOfForPolymorphism)
            {
                if (IsKnownSubType(dataContract, out var baseTypeDataContract))
                {
                    var baseTypeSchema = GenerateConcreteSchema(baseTypeDataContract, schemaRepository);

                    schema.AllOf.Add(baseTypeSchema);

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
                        schema.Properties.Add(discriminator.PropertyName, new OpenApiSchema { Type = "string" });
                        schema.Required.Add(discriminator.PropertyName);
                        schema.Discriminator = discriminator;
                    }
                }
            }

            foreach (var dataProperty in applicableDataProperties)
            {
                var customAttributes = dataProperty.MemberInfo?.GetInlineAndMetadataAttributes() ?? Enumerable.Empty<object>();

                if (_generatorOptions.IgnoreObsoleteProperties && customAttributes.OfType<ObsoleteAttribute>().Any())
                    continue;

                schema.Properties[dataProperty.Name] = GeneratePropertySchema(dataProperty, schemaRepository);

                if (dataProperty.IsRequired || customAttributes.OfType<RequiredAttribute>().Any()
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

            return schema;
        }

        private bool IsKnownSubType(DataContract dataContract, out DataContract baseTypeDataContract)
        {
            baseTypeDataContract = null;

            var baseType = dataContract.UnderlyingType.BaseType;

            if (baseType == null || baseType == typeof(object) || !_generatorOptions.SubTypesSelector(baseType).Contains(dataContract.UnderlyingType))
                return false;

            baseTypeDataContract = GetDataContractFor(baseType);
            return true;
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
                var discriminatorValue = _generatorOptions.DiscriminatorValueSelector(knownTypeDataContract.UnderlyingType)
                    ?? knownTypeDataContract.ObjectTypeNameValue;

                if (discriminatorValue == null) continue;

                discriminator.Mapping.Add(discriminatorValue, GenerateConcreteSchema(knownTypeDataContract, schemaRepository).Reference.ReferenceV3);
            }

            return true;
        }

        private OpenApiSchema GeneratePropertySchema(DataProperty dataProperty, SchemaRepository schemaRepository)
        {
            var schema = GenerateSchema(dataProperty.MemberType, schemaRepository, memberInfo: dataProperty.MemberInfo);

            if (schema.Reference == null)
            {
                schema.Nullable = schema.Nullable && dataProperty.IsNullable;
                schema.ReadOnly = dataProperty.IsReadOnly;
                schema.WriteOnly = dataProperty.IsWriteOnly;
            }

            return schema;
        }

        private OpenApiSchema GenerateReferencedSchema(
            DataContract dataContract,
            SchemaRepository schemaRepository,
            Func<OpenApiSchema> definitionFactory)
        {
            if (schemaRepository.TryLookupByType(dataContract.UnderlyingType, out OpenApiSchema referenceSchema))
                return referenceSchema;

            var schemaId = _generatorOptions.SchemaIdSelector(dataContract.UnderlyingType);

            schemaRepository.RegisterType(dataContract.UnderlyingType, schemaId);

            var schema = definitionFactory();
            ApplyFilters(schema, dataContract.UnderlyingType, schemaRepository);

            return schemaRepository.AddDefinition(schemaId, schema);
        }

        private void ApplyMemberMetadata(
            OpenApiSchema schema,
            Type type,
            MemberInfo memberInfo,
            DataContract dataContract)
        {
            if (_generatorOptions.UseAllOfToExtendReferenceSchemas && schema.Reference != null)
            {
                schema.AllOf = new[] { new OpenApiSchema { Reference = schema.Reference } };
                schema.Reference = null;
            }

            if (schema.Reference == null)
            {
                var customAttributes = memberInfo.GetInlineAndMetadataAttributes();

                schema.Nullable = type.IsReferenceOrNullableType();

                if (!_generatorOptions.SuppressNonNullableReferenceTypes && memberInfo.IsNonNullable())
                {
                    schema.Nullable = false;
                }

                var defaultValueAttribute = customAttributes.OfType<DefaultValueAttribute>().FirstOrDefault();
                if (defaultValueAttribute != null)
                {
                    var defaultAsJson = dataContract.JsonConverter(defaultValueAttribute.Value);
                    schema.Default = OpenApiAnyFactory.CreateFromJson(defaultAsJson);
                }

                var obsoleteAttribute = customAttributes.OfType<ObsoleteAttribute>().FirstOrDefault();
                if (obsoleteAttribute != null)
                {
                    schema.Deprecated = true;
                }

                schema.ApplyValidationAttributes(customAttributes);
            }
        }

        private void ApplyParameterMetadata(OpenApiSchema schema, Type type, ParameterInfo parameterInfo, DataContract dataContract)
        {
            if (_generatorOptions.UseAllOfToExtendReferenceSchemas && schema.Reference != null)
            {
                schema.AllOf = new[] { new OpenApiSchema { Reference = schema.Reference } };
                schema.Reference = null;
            }

            if (schema.Reference == null)
            {
                schema.Nullable = type.IsReferenceOrNullableType();

                if (parameterInfo.HasDefaultValue)
                {
                    var defaultAsJson = dataContract.JsonConverter(parameterInfo.DefaultValue);
                    schema.Default = OpenApiAnyFactory.CreateFromJson(defaultAsJson);
                }

                schema.ApplyValidationAttributes(parameterInfo.GetCustomAttributes());
            }
        }

        private void ApplyFilters(
            OpenApiSchema schema,
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
    }
}
