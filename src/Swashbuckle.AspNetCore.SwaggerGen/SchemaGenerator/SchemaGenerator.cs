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

        public OpenApiSchema GenerateSchema(
            Type type,
            SchemaRepository schemaRepository,
            MemberInfo memberInfo = null,
            ParameterInfo parameterInfo = null)
        {
            try
            {
                var schema = TryGetCustomTypeMapping(type, out Func<OpenApiSchema> customSchemaFactory)
                    ? customSchemaFactory()
                    : GenerateSchemaForType(type, schemaRepository);

                if (memberInfo != null)
                {
                    ApplyMemberMetadata(schema, type, memberInfo);
                }
                else if (parameterInfo != null)
                {
                    ApplyParameterMetadata(schema, type, parameterInfo);
                }

                if (schema.Reference == null)
                {
                    ApplyFilters(schema, type, schemaRepository, memberInfo, parameterInfo);
                }

                return schema;
            }
            catch (Exception ex)
            {
                throw new SchemaGeneratorException(
                    message: $"Failed to generate Schema for type - {type}. See inner exception",
                    innerException: ex);
            }
        }

        private bool TryGetCustomTypeMapping(Type type, out Func<OpenApiSchema> schemaFactory)
        {
            return _generatorOptions.CustomTypeMappings.TryGetValue(type, out schemaFactory)
                || (type.IsConstructedGenericType && _generatorOptions.CustomTypeMappings.TryGetValue(type.GetGenericTypeDefinition(), out schemaFactory));
        }

        private OpenApiSchema GenerateSchemaForType(Type type, SchemaRepository schemaRepository)
        {
            if (type.IsNullable(out Type innerType))
            {
                return GenerateSchemaForType(innerType, schemaRepository);
            }

            if (type.IsAssignableToOneOf(typeof(IFormFile), typeof(FileResult)))
            {
                return new OpenApiSchema { Type = "string", Format = "binary" };
            }

            Func<OpenApiSchema> schemaFactory;
            bool returnAsReference;

            var dataContract = _serializerBehavior.GetDataContractForType(type);

            switch (dataContract.DataType)
            {
                case DataType.Boolean:
                case DataType.Integer:
                case DataType.Number:
                case DataType.String:
                    {
                        schemaFactory = () => GeneratePrimitiveSchema(dataContract);
                        returnAsReference = type.IsEnum && !_generatorOptions.UseInlineDefinitionsForEnums;
                        break;
                    }

                case DataType.Array:
                    {
                        schemaFactory = () => GenerateArraySchema(dataContract, schemaRepository);
                        returnAsReference = type == dataContract.ArrayItemType;
                        break;
                    }

                case DataType.Dictionary:
                    {
                        schemaFactory = () => GenerateDictionarySchema(dataContract, schemaRepository);
                        returnAsReference = type == dataContract.DictionaryValueType;
                        break;
                    }

                case DataType.Object:
                    {
                        if (_generatorOptions.UseOneOfForPolymorphism && IsBaseTypeWithKnownSubTypes(type, out IEnumerable<Type> subTypes))
                        {
                            schemaFactory = () => GeneratePolymorphicSchema(dataContract, schemaRepository, subTypes);
                            returnAsReference = false;
                        }
                        else
                        {
                            schemaFactory = () => GenerateObjectSchema(dataContract, schemaRepository);
                            returnAsReference = true;
                        }

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
                ? GenerateReferencedSchema(type, schemaRepository, schemaFactory)
                : schemaFactory();
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

        private OpenApiSchema GenerateArraySchema(DataContract dataContract, SchemaRepository schemaRepository)
        {
            return new OpenApiSchema
            {
                Type = "array",
                Items = GenerateSchema(dataContract.ArrayItemType, schemaRepository),
                UniqueItems = dataContract.UnderlyingType.IsSet() ? (bool?)true : null
            };
        }

        private OpenApiSchema GenerateDictionarySchema(DataContract dataContract, SchemaRepository schemaRepository)
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
                    Properties = propertyNames.ToDictionary(name => name, name => GenerateSchema(dataContract.DictionaryValueType, schemaRepository)),
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

        private OpenApiSchema GeneratePolymorphicSchema(
            DataContract dataContract,
            SchemaRepository schemaRepository,
            IEnumerable<Type> subTypes)
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
                var assignableSchema = GenerateReferencedSchema(
                    assignableDataContract.UnderlyingType,
                    schemaRepository,
                    () => GenerateObjectSchema(assignableDataContract, schemaRepository));

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

        private OpenApiSchema GenerateObjectSchema(DataContract dataContract, SchemaRepository schemaRepository)
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
                if (IsBaseTypeWithKnownSubTypes(dataContract.UnderlyingType, out IEnumerable<Type> subTypes))
                {
                    if (_generatorOptions.UseAllOfForInheritance)
                    {
                        // Ensure a schema for all known sub types is generated and added to the repository
                        foreach (var subType in subTypes)
                        {
                            var subTypeContract = _serializerBehavior.GetDataContractForType(subType);
                            GenerateReferencedSchema(subType, schemaRepository, () => GenerateObjectSchema(subTypeContract, schemaRepository));
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
                            GenerateReferencedSchema(baseType, schemaRepository, () => GenerateObjectSchema(baseTypeContract, schemaRepository))
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
            Type type,
            SchemaRepository schemaRepository,
            Func<OpenApiSchema> definitionFactory)
        {
            if (schemaRepository.TryLookupByType(type, out OpenApiSchema referenceSchema))
                return referenceSchema;

            var schemaId = _generatorOptions.SchemaIdSelector(type);

            schemaRepository.RegisterType(type, schemaId);

            var schema = definitionFactory();
            ApplyFilters(schema, type, schemaRepository);

            return schemaRepository.AddDefinition(schemaId, schema);
        }

        private bool IsBaseTypeWithKnownSubTypes(Type type, out IEnumerable<Type> subTypes)
        {
            subTypes = _generatorOptions.SubTypesSelector(type);

            return subTypes.Any();
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

        private void ApplyMemberMetadata(OpenApiSchema schema, Type type, MemberInfo memberInfo)
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
        }

        private void ApplyParameterMetadata(OpenApiSchema schema, Type type, ParameterInfo parameterInfo)
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
                    var defaultAsJson = _serializerBehavior.Serialize(parameterInfo.DefaultValue);
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
