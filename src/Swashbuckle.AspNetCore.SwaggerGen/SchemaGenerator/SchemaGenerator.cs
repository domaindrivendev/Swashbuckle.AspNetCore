using System;
using System.Collections.Generic;
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
        private readonly IDataContractResolver _dataContractResolver;

        public SchemaGenerator(SchemaGeneratorOptions generatorOptions, IDataContractResolver dataContractResolver)
        {
            _generatorOptions = generatorOptions;
            _dataContractResolver = dataContractResolver;
        }

        public OpenApiSchema GenerateSchema(
            Type type,
            SchemaRepository schemaRepository,
            MemberInfo memberInfo = null,
            ParameterInfo parameterInfo = null)
        {
            var schema = TryGetCustomTypeMapping(type, out Func<OpenApiSchema> mapping)
                ? mapping()
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

        private bool TryGetCustomTypeMapping(Type type, out Func<OpenApiSchema> mapping)
        {
            if (_generatorOptions.CustomTypeMappings.TryGetValue(type, out mapping))
            {
                return true;
            }

            if (type.IsConstructedGenericType &&
                _generatorOptions.CustomTypeMappings.TryGetValue(type.GetGenericTypeDefinition(), out mapping))
            {
                return true;
            }

            return false;
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

            if (type == typeof(object))
            {
                return new OpenApiSchema { Type = "object" };
            }

            Func<OpenApiSchema> definitionFactory;
            bool returnAsReference;

            var dataContract = _dataContractResolver.GetDataContractForType(type);

            switch (dataContract.DataType)
            {
                case DataType.Boolean:
                case DataType.Integer:
                case DataType.Number:
                case DataType.String:
                    {
                        definitionFactory = () => GeneratePrimitiveSchema(dataContract);
                        returnAsReference = type.IsEnum && !_generatorOptions.UseInlineDefinitionsForEnums;
                        break;
                    }

                case DataType.Array:
                    {
                        definitionFactory = () => GenerateArraySchema(dataContract, schemaRepository);
                        returnAsReference = type == dataContract.ArrayItemType;
                        break;
                    }

                case DataType.Dictionary:
                    {
                        definitionFactory = () => GenerateDictionarySchema(dataContract, schemaRepository);
                        returnAsReference = type == dataContract.DictionaryValueType;
                        break;
                    }

                case DataType.Object:
                    {
                        if (_generatorOptions.UseOneOfForPolymorphism && IsBaseTypeWithKnownSubTypes(type, out IEnumerable<Type> subTypes))
                        {
                            definitionFactory = () => GeneratePolymorphicSchema(dataContract, schemaRepository, subTypes);
                            returnAsReference = false;
                        }
                        else
                        {
                            definitionFactory = () => GenerateObjectSchema(dataContract, schemaRepository);
                            returnAsReference = true;
                        }

                        break;
                    }

                default:
                    {
                        definitionFactory = () => new OpenApiSchema();
                        returnAsReference = false;
                        break;
                    }
            }

            return returnAsReference
                ? GenerateReferencedSchema(type, schemaRepository, definitionFactory)
                : definitionFactory();
        }

        private OpenApiSchema GeneratePrimitiveSchema(DataContract dataContract)
        {
            var schema = new OpenApiSchema
            {
                Type = dataContract.DataType.ToString().ToLower(CultureInfo.InvariantCulture),
                Format = dataContract.DataFormat
            };

            if (dataContract.EnumValues != null)
            {
                schema.Enum = dataContract.EnumValues
                    .Distinct()
                    .Select(value => OpenApiAnyFactory.CreateFor(schema, value))
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
            if (dataContract.DictionaryKeys != null)
            {
                return new OpenApiSchema
                {
                    Type = "object",
                    Properties = dataContract.DictionaryKeys.ToDictionary(
                        name => name,
                        name => GenerateSchema(dataContract.DictionaryValueType, schemaRepository))
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

        private bool IsBaseTypeWithKnownSubTypes(Type type, out IEnumerable<Type> subTypes)
        {
            subTypes = _generatorOptions.SubTypesResolver(type);

            return subTypes.Any();
        }

        private bool IsKnownSubType(Type type, out Type baseType)
        {
            if ((type.BaseType != null) && _generatorOptions.SubTypesResolver(type.BaseType).Contains(type))
            {
                baseType = type.BaseType;
                return true;
            }

            baseType = null;
            return false;
        }

        private OpenApiSchema GeneratePolymorphicSchema(DataContract dataContract, SchemaRepository schemaRepository, IEnumerable<Type> subTypes)
        {
            var subTypeContracts = subTypes
                .Select(subType => _dataContractResolver.GetDataContractForType(subType));

            return new OpenApiSchema
            {
                OneOf = new[] { dataContract }.Union(subTypeContracts)
                    .Select(dc => GenerateReferencedSchema(dc.UnderlyingType, schemaRepository, () => GenerateObjectSchema(dc, schemaRepository)))
                    .ToList(),

                Discriminator = TryGetDiscriminatorFor(dataContract.UnderlyingType, out string discriminator)
                    ? new OpenApiDiscriminator { PropertyName = discriminator }
                    : null
            };
        }

        private bool TryGetDiscriminatorFor(Type type, out string discriminator)
        {
            discriminator = _generatorOptions.DiscriminatorSelector(type);
            return (discriminator != null);
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
                var isBaseType = IsBaseTypeWithKnownSubTypes(dataContract.UnderlyingType, out IEnumerable<Type> subTypes);
                var isSubType = IsKnownSubType(dataContract.UnderlyingType, out Type baseType);

                if (isBaseType && _generatorOptions.UseOneOfForPolymorphism
                    && TryGetDiscriminatorFor(dataContract.UnderlyingType, out string discriminator))
                {
                    schema.Properties.Add(discriminator, new OpenApiSchema { Type = "string" });
                }

                if (isSubType && _generatorOptions.UseOneOfForPolymorphism
                    && !_generatorOptions.UseAllOfForInheritance && TryGetDiscriminatorFor(baseType, out discriminator))
                {
                    schema.Properties.Add(discriminator, new OpenApiSchema { Type = "string" });
                }

                if (isBaseType && _generatorOptions.UseAllOfForInheritance)
                {
                    // Ensure a schema for all known sub types is generated and added to the repository
                    foreach (var subType in subTypes)
                    {
                        var subTypeContract = _dataContractResolver.GetDataContractForType(subType);
                        GenerateReferencedSchema(subType, schemaRepository, () => GenerateObjectSchema(subTypeContract, schemaRepository));
                    }
                }

                if (isSubType && _generatorOptions.UseAllOfForInheritance)
                {
                    var baseTypeContract = _dataContractResolver.GetDataContractForType(baseType);
                    schema.AllOf = new List<OpenApiSchema>
                    {
                        GenerateReferencedSchema(baseType, schemaRepository, () => GenerateObjectSchema(baseTypeContract, schemaRepository))
                    };

                    // Reduce the set of properties to be defined in this schema to declared properties only
                    applicableDataProperties = applicableDataProperties
                        .Where(dataProperty => dataProperty.MemberInfo?.DeclaringType == dataContract.UnderlyingType);
                }
            }

            foreach (var dataProperty in applicableDataProperties)
            {
                var customAttributes = dataProperty.MemberInfo?.GetInlineOrMetadataTypeAttributes() ?? Enumerable.Empty<object>();

                if (_generatorOptions.IgnoreObsoleteProperties && customAttributes.OfType<ObsoleteAttribute>().Any())
                    continue;

                schema.Properties[dataProperty.Name] = GeneratePropertySchema(dataProperty, schemaRepository);

                if (dataProperty.IsRequired || customAttributes.OfType<RequiredAttribute>().Any())
                    schema.Required.Add(dataProperty.Name);
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

        private void ApplyMemberMetadata(OpenApiSchema schema, Type type, MemberInfo memberInfo)
        {
            if (schema.Reference != null && _generatorOptions.UseAllOfToExtendReferenceSchemas)
            {
                schema.AllOf = new[] { new OpenApiSchema { Reference = schema.Reference } };
                schema.Reference = null;
            }

            if (schema.Reference == null)
            {
                schema.Nullable = type.IsReferenceOrNullableType();

                schema.ApplyCustomAttributes(memberInfo.GetInlineOrMetadataTypeAttributes());
            }
        }

        private void ApplyParameterMetadata(OpenApiSchema schema, Type type, ParameterInfo parameterInfo)
        {
            if (schema.Reference != null && _generatorOptions.UseAllOfToExtendReferenceSchemas)
            {
                schema.AllOf = new[] { new OpenApiSchema { Reference = schema.Reference } };
                schema.Reference = null;
            }

            if (schema.Reference == null)
            {
                schema.Nullable = type.IsReferenceOrNullableType();

                schema.ApplyCustomAttributes(parameterInfo.GetCustomAttributes());

                if (parameterInfo.HasDefaultValue)
                {
                    schema.Default = OpenApiAnyFactory.CreateFor(schema, parameterInfo.DefaultValue);
                }
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
