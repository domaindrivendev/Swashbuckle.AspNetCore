using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class JsonObjectHandler : SchemaGeneratorHandler
    {
        private readonly SchemaGeneratorOptions _generatorOptions;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly ISchemaGenerator _schemaGenerator;

        public JsonObjectHandler(SchemaGeneratorOptions generatorOptions, JsonSerializerOptions serializerOptions, ISchemaGenerator schemaGenerator)
        {
            _generatorOptions = generatorOptions;
            _serializerOptions = serializerOptions;
            _schemaGenerator = schemaGenerator;
        }

        public override bool CanCreateSchemaFor(Type type, out bool shouldBeReferenced)
        {
            var shouldBePolymorphic = _generatorOptions.GeneratePolymorphicSchemas && _generatorOptions.SubTypesResolver(type).Any();
            shouldBeReferenced = !(type == typeof(object) || shouldBePolymorphic);
            return true;
        }

        public override OpenApiSchema CreateSchema(Type type, SchemaRepository schemaRepository)
        {
            if (_generatorOptions.GeneratePolymorphicSchemas)
            {
                var knownSubTypes = _generatorOptions.SubTypesResolver(type);
                if (knownSubTypes.Any())
                {
                    return CreatePolymorphicSchema(knownSubTypes, schemaRepository);
                }
            }

            return CreateObjectSchema(type, schemaRepository);
        }

        private OpenApiSchema CreatePolymorphicSchema(IEnumerable<Type> knownSubTypes, SchemaRepository schemaRepository)
        {
            return new OpenApiSchema
            {
                OneOf = knownSubTypes
                    .Select(subType => _schemaGenerator.GenerateSchema(subType, schemaRepository))
                    .ToList()
            };
        }

        private OpenApiSchema CreateObjectSchema(Type type, SchemaRepository schemaRepository)
        {
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new SortedSet<string>(),
                AdditionalPropertiesAllowed = false
            };

            // If it's a baseType with known subTypes, add the discriminator property
            if (_generatorOptions.GeneratePolymorphicSchemas && _generatorOptions.SubTypesResolver(type).Any())
            {
                var discriminatorName = _generatorOptions.DiscriminatorSelector(type);
                schema.Properties.Add(discriminatorName, new OpenApiSchema { Type = "string" });
                schema.Required.Add(discriminatorName);
                schema.Discriminator = new OpenApiDiscriminator { PropertyName = discriminatorName };
            }

            var serializableProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(property =>
                {
                    return
                        (property.IsPubliclyReadable() || property.IsPubliclyWritable()) &&
                        !(property.GetIndexParameters().Any()) &&
                        !(property.HasAttribute<JsonIgnoreAttribute>()) &&
                        !(_serializerOptions.IgnoreReadOnlyProperties && !property.IsPubliclyWritable());
                });

            foreach (var property in serializableProperties)
            {
                var customAttributes = property.GetInlineOrMetadataTypeAttributes();

                if (_generatorOptions.IgnoreObsoleteProperties && customAttributes.OfType<ObsoleteAttribute>().Any()) continue;

                var name = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                    ?? _serializerOptions.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;

                schema.Properties.Add(name, CreatePropertySchema(property, customAttributes, schemaRepository));

                if (customAttributes.OfType<RequiredAttribute>().Any())
                    schema.Required.Add(name);

                if (property.HasAttribute<JsonExtensionDataAttribute>() && property.PropertyType.IsDictionary(out Type keyType, out Type valueType))
                {
                    schema.AdditionalPropertiesAllowed = true;
                    schema.AdditionalProperties = _schemaGenerator.GenerateSchema(valueType, schemaRepository);
                }
            }


            // If it's a known subType, reference the baseType for inheritied properties
            if (_generatorOptions.GeneratePolymorphicSchemas && type.BaseType != null && _generatorOptions.SubTypesResolver(type.BaseType).Contains(type))
            {
                var baseType = type.BaseType;

                var baseSchemaReference = schemaRepository.GetOrAdd(
                    type: baseType,
                    schemaId: _generatorOptions.SchemaIdSelector(baseType),
                    factoryMethod: () => CreateObjectSchema(baseType, schemaRepository));

                var baseSchema = schemaRepository.Schemas[baseSchemaReference.Reference.Id];

                schema.AllOf = new[] { baseSchemaReference };

                foreach (var basePropertyName in baseSchema.Properties.Keys)
                {
                    schema.Properties.Remove(basePropertyName);
                }
            }

            return schema;
        }

        private OpenApiSchema CreatePropertySchema(PropertyInfo property, IEnumerable<object> customAttributes, SchemaRepository schemaRepository)
        {
            var typeSchema = _schemaGenerator.GenerateSchema(property.PropertyType, schemaRepository);

            // If it's a referenced/shared schema, "extend" it using allOf so that contextual metadata (e.g. property attributes) can be applied
            var propertySchema = (typeSchema.Reference != null)
                ? new OpenApiSchema { AllOf = new[] { typeSchema } }
                : typeSchema;

            propertySchema.ReadOnly = (property.IsPubliclyReadable() && !property.IsPubliclyWritable());
            propertySchema.WriteOnly = (!property.IsPubliclyReadable() && property.IsPubliclyWritable());
            propertySchema.Nullable = property.PropertyType.IsReferenceOrNullableType();

            propertySchema.ApplyCustomAttributes(customAttributes);

            return propertySchema;
        }
    }
}