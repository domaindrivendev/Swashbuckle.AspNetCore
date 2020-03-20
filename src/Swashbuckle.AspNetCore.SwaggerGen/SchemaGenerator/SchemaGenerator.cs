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
        private readonly ISerializerMetadataResolver _serializerMetadataResolver;

        public SchemaGenerator(SchemaGeneratorOptions generatorOptions, ISerializerMetadataResolver serializerMetadataResolver)
        {
            _generatorOptions = generatorOptions;
            _serializerMetadataResolver = serializerMetadataResolver;
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

            var serializerMetadata = _serializerMetadataResolver.GetSerializerMetadataForType(type);

            var shouldBeReferened = (serializerMetadata.EnumValues != null && !_generatorOptions.UseInlineDefinitionsForEnums)
                || (serializerMetadata.IsDictionary && serializerMetadata.Type == serializerMetadata.DictionaryValueType)
                || (serializerMetadata.IsArray && serializerMetadata.Type == serializerMetadata.ArrayItemType)
                || (serializerMetadata.IsObject && serializerMetadata.Properties != null);

            return (shouldBeReferened)
                ? GenerateReferencedSchema(serializerMetadata, schemaRepository)
                : GenerateInlineSchema(serializerMetadata, schemaRepository);
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

        private OpenApiSchema GenerateReferencedSchema(SerializerMetadata serializerMetadata, SchemaRepository schemaRepository)
        {
            return schemaRepository.GetOrAdd(
                serializerMetadata.Type,
                _generatorOptions.SchemaIdSelector(serializerMetadata.Type),
                () =>
                {
                    var schema = GenerateInlineSchema(serializerMetadata, schemaRepository);
                    ApplyFilters(schema, serializerMetadata.Type, schemaRepository);
                    return schema;
                });
        }

        private OpenApiSchema GenerateInlineSchema(SerializerMetadata serializerMetadata, SchemaRepository schemaRepository)
        {
            if (serializerMetadata.IsPrimitive)
                return GeneratePrimitiveSchema(serializerMetadata);

            if (serializerMetadata.IsDictionary)
                return GenerateDictionarySchema(serializerMetadata, schemaRepository);

            if (serializerMetadata.IsArray)
                return GenerateArraySchema(serializerMetadata, schemaRepository);

            if (serializerMetadata.IsObject)
                return GenerateObjectSchema(serializerMetadata, schemaRepository);

            return new OpenApiSchema();
        }

        private OpenApiSchema GeneratePrimitiveSchema(SerializerMetadata serializerMetadata)
        {
            var schema = new OpenApiSchema
            {
                Type = serializerMetadata.DataType,
                Format = serializerMetadata.DataFormat
            };

            if (serializerMetadata.EnumValues != null)
            {
                schema.Enum = serializerMetadata.EnumValues
                    .Select(value => OpenApiAnyFactory.CreateFor(schema, value))
                    .ToList();
            }

            return schema;
        }

        private OpenApiSchema GenerateDictionarySchema(SerializerMetadata serializerMetadata, SchemaRepository schemaRepository)
        {
            if (serializerMetadata.DictionaryKeyType.IsEnum)
            {
                // This is a special case where we can include named properties based on the enum values
                return new OpenApiSchema
                {
                    Type = "object",
                    Properties = serializerMetadata.DictionaryKeyType.GetEnumNames()
                        .ToDictionary(
                            name => name,
                            name => GenerateSchema(serializerMetadata.DictionaryValueType, schemaRepository)
                        )
                };
            }

            return new OpenApiSchema
            {
                Type = "object",
                AdditionalPropertiesAllowed = true,
                AdditionalProperties = GenerateSchema(serializerMetadata.DictionaryValueType, schemaRepository)
            };
        }

        private OpenApiSchema GenerateArraySchema(SerializerMetadata serializerMetadata, SchemaRepository schemaRepository)
        {
            return new OpenApiSchema
            {
                Type = "array",
                Items = GenerateSchema(serializerMetadata.ArrayItemType, schemaRepository),
                UniqueItems = serializerMetadata.Type.IsSet() ? (bool?)true : null
            };
        }

        private OpenApiSchema GenerateObjectSchema(SerializerMetadata serializerMetadata, SchemaRepository schemaRepository)
        {
            if (serializerMetadata.Properties == null)
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
            if (_generatorOptions.GeneratePolymorphicSchemas && _generatorOptions.SubTypesResolver(serializerMetadata.Type).Any())
            {
                var discriminatorName = _generatorOptions.DiscriminatorSelector(serializerMetadata.Type);
                schema.Properties.Add(discriminatorName, new OpenApiSchema { Type = "string" });
                schema.Required.Add(discriminatorName);
                schema.Discriminator = new OpenApiDiscriminator { PropertyName = discriminatorName };
            }

            foreach (var serializerPropertyMetadata in serializerMetadata.Properties)
            {
                var customAttributes = serializerPropertyMetadata.MemberInfo?.GetInlineOrMetadataTypeAttributes();

                if (_generatorOptions.IgnoreObsoleteProperties && customAttributes.OfType<ObsoleteAttribute>().Any())
                    continue;

                var propertySchema = GenerateSchema(serializerPropertyMetadata.MemberType, schemaRepository, memberInfo: serializerPropertyMetadata.MemberInfo);

                schema.Properties.Add(serializerPropertyMetadata.Name, propertySchema);

                if (serializerPropertyMetadata.IsRequired || customAttributes.OfType<RequiredAttribute>().Any())
                    schema.Required.Add(serializerPropertyMetadata.Name);

                if (propertySchema.Reference == null)
                {
                    propertySchema.Nullable = propertySchema.Nullable && serializerPropertyMetadata.AllowNull;
                }
            }

            if (serializerMetadata.ExtensionDataValueType != null)
            {
                schema.AdditionalProperties = GenerateSchema(serializerMetadata.ExtensionDataValueType, schemaRepository);
            }

            // If it's a known subType, reference the baseType for inheritied properties
            if (_generatorOptions.GeneratePolymorphicSchemas
                && (serializerMetadata.Type.BaseType != null)
                && _generatorOptions.SubTypesResolver(serializerMetadata.Type.BaseType).Contains(serializerMetadata.Type))
            {
                var baseSerializerContract = _serializerMetadataResolver.GetSerializerMetadataForType(serializerMetadata.Type.BaseType);
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

                if (memberInfo is PropertyInfo propertyInfo)
                {
                    schema.ReadOnly = (propertyInfo.IsPubliclyReadable() && !propertyInfo.IsPubliclyWritable());
                    schema.WriteOnly = (!propertyInfo.IsPubliclyReadable() && propertyInfo.IsPubliclyWritable());
                }
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
