using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

            var shouldBeReferened = (dataContract.EnumValues != null && !_generatorOptions.UseInlineDefinitionsForEnums)
                || (dataContract.IsDictionary && dataContract.Type == dataContract.DictionaryValueType)
                || (dataContract.IsArray && dataContract.Type == dataContract.ArrayItemType)
                || (dataContract.IsObject && dataContract.Members != null);

            return (shouldBeReferened)
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
                dataContract.Type,
                _generatorOptions.SchemaIdSelector(dataContract.Type),
                () =>
                {
                    var schema = GenerateInlineSchema(dataContract, schemaRepository);
                    ApplyFilters(schema, dataContract.Type, schemaRepository);
                    return schema;
                });
        }

        private OpenApiSchema GenerateInlineSchema(DataContract dataContract, SchemaRepository schemaRepository)
        {
            if (dataContract.IsPrimitive)
                return GeneratePrimitiveSchema(dataContract);

            if (dataContract.IsDictionary)
                return GenerateDictionarySchema(dataContract, schemaRepository);

            if (dataContract.IsArray)
                return GenerateArraySchema(dataContract, schemaRepository);

            if (dataContract.IsObject)
                return GenerateObjectSchema(dataContract, schemaRepository);

            return new OpenApiSchema();
        }

        private OpenApiSchema GeneratePrimitiveSchema(DataContract dataContract)
        {
            var schema = new OpenApiSchema
            {
                Type = dataContract.DataType,
                Format = dataContract.DataFormat
            };

            if (dataContract.EnumValues != null)
            {
                schema.Enum = dataContract.EnumValues
                    .Select(value => OpenApiAnyFactory.CreateFor(schema, value))
                    .ToList();
            }

            return schema;
        }

        private OpenApiSchema GenerateDictionarySchema(DataContract dataContract, SchemaRepository schemaRepository)
        {
            if (dataContract.DictionaryKeyType.IsEnum)
            {
                // This is a special case where we can include named properties based on the enum values
                return new OpenApiSchema
                {
                    Type = "object",
                    Properties = dataContract.DictionaryKeyType.GetEnumNames()
                        .ToDictionary(
                            name => name,
                            name => GenerateSchema(dataContract.DictionaryValueType, schemaRepository)
                        )
                };
            }

            return new OpenApiSchema
            {
                Type = "object",
                AdditionalPropertiesAllowed = true,
                AdditionalProperties = GenerateSchema(dataContract.DictionaryValueType, schemaRepository)
            };
        }

        private OpenApiSchema GenerateArraySchema(DataContract dataContract, SchemaRepository schemaRepository)
        {
            return new OpenApiSchema
            {
                Type = "array",
                Items = GenerateSchema(dataContract.ArrayItemType, schemaRepository),
                UniqueItems = dataContract.Type.IsSet() ? (bool?)true : null
            };
        }

        private OpenApiSchema GenerateObjectSchema(DataContract dataContract, SchemaRepository schemaRepository)
        {
            if (dataContract.Members == null)
            {
                return new OpenApiSchema { Type = "object" };
            }

            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new SortedSet<string>()
            };

            // If it's a baseType with known subTypes, add the discriminator property
            if (_generatorOptions.GeneratePolymorphicSchemas && _generatorOptions.SubTypesResolver(dataContract.Type).Any())
            {
                var discriminatorName = _generatorOptions.DiscriminatorSelector(dataContract.Type);

                if (!schema.Properties.ContainsKey(discriminatorName))
                    schema.Properties.Add(discriminatorName, new OpenApiSchema { Type = "string" });

                schema.Required.Add(discriminatorName);
                schema.Discriminator = new OpenApiDiscriminator { PropertyName = discriminatorName };
            }

            foreach (var dataMember in dataContract.Members)
            {
                var customAttributes = dataMember.MemberInfo?.GetInlineOrMetadataTypeAttributes() ?? Enumerable.Empty<object>();

                if (_generatorOptions.IgnoreObsoleteProperties && customAttributes.OfType<ObsoleteAttribute>().Any())
                    continue;

                schema.Properties[dataMember.Name] = GeneratePropertySchema(dataMember, schemaRepository);

                if (dataMember.IsRequired || customAttributes.OfType<RequiredAttribute>().Any())
                    schema.Required.Add(dataMember.Name);
            }

            if (dataContract.ExtensionDataValueType != null)
            {
                schema.AdditionalProperties = GenerateSchema(dataContract.ExtensionDataValueType, schemaRepository);
            }

            // If it's a known subType, reference the baseType for inheritied properties
            if (_generatorOptions.GeneratePolymorphicSchemas
                && (dataContract.Type.BaseType != null)
                && _generatorOptions.SubTypesResolver(dataContract.Type.BaseType).Contains(dataContract.Type))
            {
                var basedataContract = _dataContractResolver.GetDataContractForType(dataContract.Type.BaseType);
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

        private OpenApiSchema GeneratePropertySchema(DataMember serializerMember, SchemaRepository schemaRepository)
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
