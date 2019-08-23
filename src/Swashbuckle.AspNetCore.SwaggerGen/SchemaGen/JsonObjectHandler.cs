using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class JsonObjectHandler : SchemaGeneratorHandler
    {
        public JsonObjectHandler(SchemaGeneratorOptions schemaGeneratorOptions, ISchemaGenerator schemaGenerator)
            : base(schemaGeneratorOptions, schemaGenerator)
        { }

        protected override bool CanGenerateSchema(JsonContract jsonContract, out bool shouldBeReferenced)
        {
            if (jsonContract is JsonObjectContract jsonObjectContract)
            {
                shouldBeReferenced = (jsonObjectContract.UnderlyingType != typeof(object));
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        protected override OpenApiSchema GenerateDefinitionSchema(JsonContract jsonContract, SchemaRepository schemaRepository)
        {
            var jsonObjectContract = (JsonObjectContract)jsonContract;

            if (jsonObjectContract.UnderlyingType == typeof(object))
            {
                return new OpenApiSchema { Type = "object" };
            }

            var additionalProperties = (jsonObjectContract.ExtensionDataValueType != null)
                ? SchemaGenerator.GenerateSchema(jsonObjectContract.ExtensionDataValueType, schemaRepository)
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
                var memberAttributes = jsonProperty.TryGetMemberInfo(out MemberInfo memberInfo)
                    ? memberInfo.GetCustomAttributes(true)
                    : Enumerable.Empty<object>();

                if (memberAttributes.OfType<ObsoleteAttribute>().Any() || jsonProperty.Ignored)
                {
                    continue;
                }

                schema.Properties.Add(jsonProperty.PropertyName, GeneratePropertySchema(jsonProperty, schemaRepository));

                if (memberAttributes.OfType<RequiredAttribute>().Any()
                    || jsonProperty.Required == Required.AllowNull
                    || jsonProperty.Required == Required.Always)
                {
                    schema.Required.Add(jsonProperty.PropertyName);
                }
            }

            return schema;
        }

        private OpenApiSchema GeneratePropertySchema(JsonProperty jsonProperty, SchemaRepository schemaRepository)
        {
            var schema = SchemaGenerator.GenerateSchema(jsonProperty.PropertyType, schemaRepository);

            // If it's a reference schema exit as contextual metadata can't be applied
            if (schema.Reference != null || !jsonProperty.TryGetMemberInfo(out MemberInfo memberInfo))
                return schema;

            schema.ApplyCustomAttributes(memberInfo.GetCustomAttributes(true));

            if (memberInfo is PropertyInfo propertyInfo)
            {
                schema.ReadOnly = (propertyInfo.CanRead && !propertyInfo.CanWrite);
                schema.WriteOnly = (propertyInfo.CanWrite && !propertyInfo.CanRead);
            }

            return schema;
        }
    }
}