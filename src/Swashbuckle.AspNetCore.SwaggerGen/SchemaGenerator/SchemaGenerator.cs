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
            var schema = GenerateSchemaForType(type, schemaRepository);

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

        private OpenApiSchema GenerateSchemaForType(Type type, SchemaRepository schemaRepository)
        {
            if (TryGetCustomMapping(type, out var mapping))
            {
                return mapping();
            }

            if (type.IsAssignableToOneOf(typeof(IFormFile), typeof(FileResult)))
            {
                return new OpenApiSchema { Type = "string", Format = "binary" };
            }

            if (_generatorOptions.GeneratePolymorphicSchemas)
            {
                var knownSubTypes = _generatorOptions.SubTypesResolver(type);
                if (knownSubTypes.Any())
                {
                    return GeneratePolymorphicSchema(knownSubTypes, schemaRepository);
                }
            }

            var dataContract = _dataContractResolver.GetDataContractForType(type);

            var shouldBeReferenced =
                // regular object
                (dataContract.DataType == DataType.Object && dataContract.Properties != null && !dataContract.UnderlyingType.IsDictionary()) ||
                // dictionary-based AND self-referencing
                (dataContract.DataType == DataType.Object && dataContract.AdditionalPropertiesType == dataContract.UnderlyingType) ||
                // array-based AND self-referencing
                (dataContract.DataType == DataType.Array && dataContract.ArrayItemType == dataContract.UnderlyingType) ||
                // enum-based AND opted-out of inline
                (dataContract.EnumValues != null && !_generatorOptions.UseInlineDefinitionsForEnums);

            return (shouldBeReferenced)
                ? GenerateReferencedSchema(dataContract, schemaRepository)
                : GenerateInlineSchema(dataContract, schemaRepository);
        }

        private bool TryGetCustomMapping(Type type, out Func<OpenApiSchema> mapping)
        {
            if (_generatorOptions.CustomTypeMappings.TryGetValue(type, out mapping))
            {
                return true;
            }

            if (type.IsGenericType && !type.IsGenericTypeDefinition &&
                _generatorOptions.CustomTypeMappings.TryGetValue(type.GetGenericTypeDefinition(), out mapping))
            {
                return true;
            }

            return false;
        }

        private OpenApiSchema GeneratePolymorphicSchema(IEnumerable<Type> knownSubTypes, SchemaRepository schemaRepository)
        {
            return new OpenApiSchema
            {
                OneOf = knownSubTypes
                    .Select(subType => GenerateSchema(subType, schemaRepository))
                    .ToList()
            };
        }

        private OpenApiSchema GenerateReferencedSchema(DataContract dataContract, SchemaRepository schemaRepository)
        {
            return schemaRepository.GetOrAdd(
                dataContract.UnderlyingType,
                _generatorOptions.SchemaIdSelector(dataContract.UnderlyingType),
                () =>
                {
                    var schema = GenerateInlineSchema(dataContract, schemaRepository);
                    ApplyFilters(schema, dataContract.UnderlyingType, schemaRepository);
                    return schema;
                });
        }

        private OpenApiSchema GenerateInlineSchema(DataContract dataContract, SchemaRepository schemaRepository)
        {
            if (dataContract.DataType == DataType.Unknown)
                return new OpenApiSchema();

            if (dataContract.DataType == DataType.Object)
                return GenerateObjectSchema(dataContract, schemaRepository);

            if (dataContract.DataType == DataType.Array)
                return GenerateArraySchema(dataContract, schemaRepository);

            else
                return GeneratePrimitiveSchema(dataContract);
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

            // If it's a baseType with known subTypes, add the discriminator property
            if (_generatorOptions.GeneratePolymorphicSchemas && _generatorOptions.SubTypesResolver(dataContract.UnderlyingType).Any())
            {
                var discriminatorName = _generatorOptions.DiscriminatorSelector(dataContract.UnderlyingType);

                if (!schema.Properties.ContainsKey(discriminatorName))
                    schema.Properties.Add(discriminatorName, new OpenApiSchema { Type = "string" });

                schema.Required.Add(discriminatorName);
                schema.Discriminator = new OpenApiDiscriminator { PropertyName = discriminatorName };
            }

            foreach (var dataProperty in dataContract.Properties ?? Enumerable.Empty<DataProperty>())
            {
                var customAttributes = dataProperty.MemberInfo?.GetInlineOrMetadataTypeAttributes() ?? Enumerable.Empty<object>();

                if (_generatorOptions.IgnoreObsoleteProperties && customAttributes.OfType<ObsoleteAttribute>().Any())
                    continue;

                schema.Properties[dataProperty.Name] = GeneratePropertySchema(dataProperty, schemaRepository);

                if (dataProperty.IsRequired || customAttributes.OfType<RequiredAttribute>().Any())
                    schema.Required.Add(dataProperty.Name);
            }

            if (dataContract.AdditionalPropertiesType != null)
            {
                schema.AdditionalPropertiesAllowed = true;
                schema.AdditionalProperties = GenerateSchema(dataContract.AdditionalPropertiesType, schemaRepository);
            }

            // If it's a known subType, reference the baseType for inheritied properties
            if (
                _generatorOptions.GeneratePolymorphicSchemas &&
                (dataContract.UnderlyingType.BaseType != null) &&
                _generatorOptions.SubTypesResolver(dataContract.UnderlyingType.BaseType).Contains(dataContract.UnderlyingType))
            {
                var basedataContract = _dataContractResolver.GetDataContractForType(dataContract.UnderlyingType.BaseType);
                var baseSchemaReference = GenerateReferencedSchema(basedataContract, schemaRepository);

                var baseSchema = schemaRepository.Schemas[baseSchemaReference.Reference.Id];
                foreach (var basePropertyName in baseSchema.Properties.Keys)
                {
                    schema.Properties.Remove(basePropertyName);
                }

                return new OpenApiSchema
                {
                    AllOf = new List<OpenApiSchema> { baseSchemaReference, schema }
                };
            }

            return schema;
        }

        private OpenApiSchema GeneratePropertySchema(DataProperty serializerMember, SchemaRepository schemaRepository)
        {
            var schema = GenerateSchemaForType(serializerMember.MemberType, schemaRepository);

            if (serializerMember.MemberInfo != null)
            {
                ApplyMemberMetadata(schema, serializerMember.MemberType, serializerMember.MemberInfo);
            }

            if (schema.Reference == null)
            {
                schema.Nullable = serializerMember.IsNullable && schema.Nullable;
                schema.ReadOnly = serializerMember.IsReadOnly;
                schema.WriteOnly = serializerMember.IsWriteOnly;

                ApplyFilters(schema, serializerMember.MemberType, schemaRepository, serializerMember.MemberInfo);
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

        private OpenApiSchema GeneratePrimitiveSchema(DataContract dataContract)
        {
            var schema = new OpenApiSchema
            {
                Type = dataContract.DataType.ToString().ToLower(CultureInfo.InvariantCulture),
                Format = dataContract.Format
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
