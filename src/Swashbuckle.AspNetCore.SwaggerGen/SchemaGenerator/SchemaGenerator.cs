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
        private readonly ISerializerContractResolver _serializerContractResolver;

        public SchemaGenerator(SchemaGeneratorOptions generatorOptions, ISerializerContractResolver serializerContractResolver)
        {
            _generatorOptions = generatorOptions;
            _serializerContractResolver = serializerContractResolver;
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
            if (_generatorOptions.CustomTypeMappings.ContainsKey(type))
            {
                return _generatorOptions.CustomTypeMappings[type]();
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

            var serializerContract = _serializerContractResolver.GetSerializerContractForType(type);

            var shouldBeReferened = (serializerContract.EnumValues != null && !_generatorOptions.UseInlineDefinitionsForEnums)
                || (serializerContract.IsDictionary && serializerContract.Type == serializerContract.DictionaryValueType)
                || (serializerContract.IsArray && serializerContract.Type == serializerContract.ArrayItemType)
                || (serializerContract.IsObject && serializerContract.Members != null);

            return (shouldBeReferened)
                ? GenerateReferencedSchema(serializerContract, schemaRepository)
                : GenerateInlineSchema(serializerContract, schemaRepository);
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

        private OpenApiSchema GenerateReferencedSchema(SerializerContract serializerContract, SchemaRepository schemaRepository)
        {
            return schemaRepository.GetOrAdd(
                serializerContract.Type,
                _generatorOptions.SchemaIdSelector(serializerContract.Type),
                () =>
                {
                    var schema = GenerateInlineSchema(serializerContract, schemaRepository);
                    ApplyFilters(schema, serializerContract.Type, schemaRepository);
                    return schema;
                });
        }

        private OpenApiSchema GenerateInlineSchema(SerializerContract serializerContract, SchemaRepository schemaRepository)
        {
            if (serializerContract.IsPrimitive)
                return GeneratePrimitiveSchema(serializerContract);

            if (serializerContract.IsDictionary)
                return GenerateDictionarySchema(serializerContract, schemaRepository);

            if (serializerContract.IsArray)
                return GenerateArraySchema(serializerContract, schemaRepository);

            if (serializerContract.IsObject)
                return GenerateObjectSchema(serializerContract, schemaRepository);

            return new OpenApiSchema();
        }

        private OpenApiSchema GeneratePrimitiveSchema(SerializerContract serializerContract)
        {
            var schema = new OpenApiSchema
            {
                Type = serializerContract.DataType,
                Format = serializerContract.DataFormat
            };

            if (serializerContract.EnumValues != null)
            {
                schema.Enum = serializerContract.EnumValues
                    .Select(value => OpenApiAnyFactory.CreateFor(schema, value))
                    .ToList();
            }

            return schema;
        }

        private OpenApiSchema GenerateDictionarySchema(SerializerContract serializerContract, SchemaRepository schemaRepository)
        {
            if (serializerContract.DictionaryKeyType.IsEnum)
            {
                // This is a special case where we can include named properties based on the enum values
                return new OpenApiSchema
                {
                    Type = "object",
                    Properties = serializerContract.DictionaryKeyType.GetEnumNames()
                        .ToDictionary(
                            name => name,
                            name => GenerateSchema(serializerContract.DictionaryValueType, schemaRepository)
                        )
                };
            }

            return new OpenApiSchema
            {
                Type = "object",
                AdditionalPropertiesAllowed = true,
                AdditionalProperties = GenerateSchema(serializerContract.DictionaryValueType, schemaRepository)
            };
        }

        private OpenApiSchema GenerateArraySchema(SerializerContract serializerContract, SchemaRepository schemaRepository)
        {
            return new OpenApiSchema
            {
                Type = "array",
                Items = GenerateSchema(serializerContract.ArrayItemType, schemaRepository),
                UniqueItems = serializerContract.Type.IsSet() ? (bool?)true : null
            };
        }

        private OpenApiSchema GenerateObjectSchema(SerializerContract serializerContract, SchemaRepository schemaRepository)
        {
            if (serializerContract.Members == null)
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
            if (_generatorOptions.GeneratePolymorphicSchemas && _generatorOptions.SubTypesResolver(serializerContract.Type).Any())
            {
                var discriminatorName = _generatorOptions.DiscriminatorSelector(serializerContract.Type);

                if (!schema.Properties.ContainsKey(discriminatorName))
                    schema.Properties.Add(discriminatorName, new OpenApiSchema { Type = "string" });

                schema.Required.Add(discriminatorName);
                schema.Discriminator = new OpenApiDiscriminator { PropertyName = discriminatorName };
            }

            foreach (var serializerMember in serializerContract.Members)
            {
                var customAttributes = serializerMember.MemberInfo?.GetInlineOrMetadataTypeAttributes() ?? Enumerable.Empty<object>();

                if (_generatorOptions.IgnoreObsoleteProperties && customAttributes.OfType<ObsoleteAttribute>().Any())
                    continue;

                var propertySchema = GenerateSchema(serializerMember.MemberType, schemaRepository, memberInfo: serializerMember.MemberInfo);

                schema.Properties[serializerMember.Name] = propertySchema;

                if (serializerMember.IsRequired || customAttributes.OfType<RequiredAttribute>().Any())
                    schema.Required.Add(serializerMember.Name);

                if (propertySchema.Reference == null)
                {
                    propertySchema.Nullable = serializerMember.IsNullable && propertySchema.Nullable;
                    propertySchema.ReadOnly = serializerMember.IsReadOnly;
                    propertySchema.WriteOnly = serializerMember.IsWriteOnly;
                }
            }

            if (serializerContract.ExtensionDataValueType != null)
            {
                schema.AdditionalProperties = GenerateSchema(serializerContract.ExtensionDataValueType, schemaRepository);
            }

            // If it's a known subType, reference the baseType for inheritied properties
            if (_generatorOptions.GeneratePolymorphicSchemas
                && (serializerContract.Type.BaseType != null)
                && _generatorOptions.SubTypesResolver(serializerContract.Type.BaseType).Contains(serializerContract.Type))
            {
                var baseSerializerContract = _serializerContractResolver.GetSerializerContractForType(serializerContract.Type.BaseType);
                var baseSchemaReference = GenerateReferencedSchema(baseSerializerContract, schemaRepository);

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
