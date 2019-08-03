using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class JsonObjectHandler : SchemaGeneratorHandler
    {
        public JsonObjectHandler(SchemaGeneratorOptions schemaGeneratorOptions, SchemaGenerator schemaGenerator, JsonSerializerSettings jsonSerializerSettings)
            : base(schemaGeneratorOptions, schemaGenerator, jsonSerializerSettings)
        { }

        protected override bool CanGenerateSchemaFor(ModelMetadata modelMetadata, JsonContract jsonContract)
        {
            return jsonContract is JsonObjectContract;
        }

        protected override OpenApiSchema GenerateSchemaFor(ModelMetadata modelMetadata, SchemaRepository schemaRepository, JsonContract jsonContract)
        {
            var jsonObjectContract = (JsonObjectContract)jsonContract;

            var additionalProperties = (jsonObjectContract.ExtensionDataValueType != null)
                ? SchemaGenerator.GenerateSchema(modelMetadata.GetMetadataForType(jsonObjectContract.ExtensionDataValueType), schemaRepository)
                : null;

            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new SortedSet<string>(),
                AdditionalPropertiesAllowed = (additionalProperties != null),
                AdditionalProperties = additionalProperties
            };

            foreach (var jsonProperty in jsonObjectContract.Properties)
            {
                var propertyMetadata = modelMetadata.Properties[jsonProperty.UnderlyingName]
                    ?? modelMetadata.GetMetadataForType(jsonProperty.PropertyType);

                var propertyAttributes = propertyMetadata.GetCustomAttributes();

                if (propertyAttributes.OfType<ObsoleteAttribute>().Any() || jsonProperty.Ignored) continue;

                schema.Properties.Add(jsonProperty.PropertyName, GeneratePropertySchemaFor(propertyMetadata, schemaRepository, jsonProperty));

                if (propertyAttributes.OfType<RequiredAttribute>().Any()
                    || jsonProperty.Required == Required.AllowNull
                    || jsonProperty.Required == Required.Always)
                {
                    schema.Required.Add(jsonProperty.PropertyName);
                }
            }

            return schema;
        }

        private OpenApiSchema GeneratePropertySchemaFor(
            ModelMetadata modelMetadata,
            SchemaRepository schemaRepository,
            JsonProperty jsonProperty)
        {
            var schema = SchemaGenerator.GenerateSchema(modelMetadata, schemaRepository);

            // Only apply contextual metadata (e.g. property attributes etc.) if it's an inline definition
            if (schema.Reference == null)
            {
                schema.ApplyCustomAttributes(modelMetadata.GetCustomAttributes());

                schema.WriteOnly = jsonProperty.Writable && !jsonProperty.Readable;
                schema.ReadOnly = jsonProperty.Readable && !jsonProperty.Writable;
            }

            return schema;
        }
    }
}