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
            // It's the last handler in the chain so assume object type by process of elimination
            shouldBeReferenced = true;
            return true;
        }

        public override OpenApiSchema CreateDefinitionSchema(Type type, SchemaRepository schemaRepository)
        {
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new SortedSet<string>(),
                Nullable = !_serializerOptions.IgnoreNullValues
            };

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

                schema.Properties.Add(name, GeneratePropertySchema(property, customAttributes, schemaRepository));

                if (customAttributes.OfType<RequiredAttribute>().Any())
                    schema.Required.Add(name);

                if (property.HasAttribute<JsonExtensionDataAttribute>() && property.PropertyType.IsDictionary(out Type keyType, out Type valueType))
                {
                    schema.AdditionalPropertiesAllowed = true;
                    schema.AdditionalProperties = _schemaGenerator.GenerateSchema(valueType, schemaRepository);
                }
            }

            return schema;
        }

        private OpenApiSchema GeneratePropertySchema(PropertyInfo property, IEnumerable<object> customAttributes, SchemaRepository schemaRepository)
        {
            var propertySchema = _schemaGenerator.GenerateSchema(property.PropertyType, schemaRepository);

            //If it's NOT a reference schema, apply contextual metadata (i.e. from MemberInfo)
            if (propertySchema.Reference == null)
            {
                propertySchema.ReadOnly = (property.IsPubliclyReadable() && !property.IsPubliclyWritable());
                propertySchema.WriteOnly = (!property.IsPubliclyReadable() && property.IsPubliclyWritable());

                propertySchema.ApplyCustomAttributes(customAttributes);
            }

            return propertySchema;
        }
    }
}