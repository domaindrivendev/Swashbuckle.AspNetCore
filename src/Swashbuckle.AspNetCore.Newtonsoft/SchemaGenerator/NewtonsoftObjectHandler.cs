using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Newtonsoft
{
    public class NewtonsoftObjectHandler : SchemaGeneratorHandler
    {
        private readonly SchemaGeneratorOptions _generatorOptions;
        private readonly IContractResolver _contractResolver;
        private readonly ISchemaGenerator _schemaGenerator;

        public NewtonsoftObjectHandler(
            SchemaGeneratorOptions generatorOptions,
            IContractResolver contractResolver,
            ISchemaGenerator schemaGenerator)
        {
            _generatorOptions = generatorOptions;
            _contractResolver = contractResolver;
            _schemaGenerator = schemaGenerator;
        }

        public override bool CanCreateSchemaFor(Type type, out bool shouldBeReferenced)
        {
            if (_contractResolver.ResolveContract(type) is JsonObjectContract)
            {
                shouldBeReferenced = (type != typeof(object));
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        public override OpenApiSchema CreateDefinitionSchema(Type type, SchemaRepository schemaRepository)
        {
            if (!(_contractResolver.ResolveContract(type) is JsonObjectContract jsonObjectContract))
               throw new InvalidOperationException($"Type {type} does not resolve to a JsonObjectContract");

            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new SortedSet<string>()
            };

            foreach (var jsonProperty in jsonObjectContract.Properties)
            {
                if (jsonProperty.Ignored) continue;

                var customAttributes = jsonProperty.TryGetMemberInfo(out MemberInfo memberInfo)
                    ? memberInfo.GetInlineOrMetadataTypeAttributes()
                    : Enumerable.Empty<object>();

                if (_generatorOptions.IgnoreObsoleteProperties && customAttributes.OfType<ObsoleteAttribute>().Any()) continue;

                var required = jsonProperty.IsRequiredSpecified()
                    ? jsonProperty.Required
                    : jsonObjectContract.ItemRequired ?? Required.Default;

                schema.Properties.Add(jsonProperty.PropertyName, GeneratePropertySchema(jsonProperty, customAttributes, required, schemaRepository));

                if (required == Required.Always || required == Required.AllowNull || customAttributes.OfType<RequiredAttribute>().Any())
                {
                    schema.Required.Add(jsonProperty.PropertyName);
                }
            }

            if (jsonObjectContract.ExtensionDataValueType != null)
            {
                schema.AdditionalProperties = _schemaGenerator.GenerateSchema(jsonObjectContract.ExtensionDataValueType, schemaRepository);
                schema.AdditionalPropertiesAllowed = true;
            }

            return schema;
        }

        private OpenApiSchema GeneratePropertySchema(
            JsonProperty jsonProperty,
            IEnumerable<object> customAttributes,
            Required required,
            SchemaRepository schemaRepository)
        {
            var typeSchema = _schemaGenerator.GenerateSchema(jsonProperty.PropertyType, schemaRepository);

            // If it's a referenced/shared schema, "extend" it using allOf so that contextual metadata (e.g. property attributes) can be applied
            var propertySchema = (typeSchema.Reference != null)
                ? new OpenApiSchema { AllOf = new[] { typeSchema } }
                : typeSchema;

            propertySchema.ReadOnly = jsonProperty.Readable && !jsonProperty.Writable;
            propertySchema.WriteOnly = !jsonProperty.Readable && jsonProperty.Writable;
            propertySchema.Nullable = (required == Required.Default || required == Required.AllowNull) && jsonProperty.PropertyType.IsReferenceOrNullableType();

            propertySchema.ApplyCustomAttributes(customAttributes);

            return propertySchema;
        }
    }
}