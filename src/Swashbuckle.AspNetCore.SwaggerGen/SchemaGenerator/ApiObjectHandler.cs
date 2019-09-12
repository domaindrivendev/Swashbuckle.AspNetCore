using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class ApiObjectHandler : ApiModelHandler
    {
        public ApiObjectHandler(SchemaGeneratorOptions schemaGeneratorOptions, ISchemaGenerator schemaGenerator)
            : base(schemaGeneratorOptions, schemaGenerator)
        { }

        protected override bool CanGenerateSchema(ApiModel apiModel, out bool shouldBeReferenced)
        {
            if (apiModel is ApiObject apiObject)
            {
                shouldBeReferenced = (apiObject.Type != typeof(object));
                return true;
            }

            shouldBeReferenced = false;
            return false;
        }

        protected override OpenApiSchema GenerateDefinitionSchema(ApiModel apiModel, SchemaRepository schemaRepository)
        {
            var apiObject = (ApiObject)apiModel;

            var additionalProperties = (apiObject.AdditionalPropertiesType != null)
                ? Generator.GenerateSchema(apiObject.AdditionalPropertiesType, schemaRepository)
                : null;

            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new SortedSet<string>(),
                AdditionalPropertiesAllowed = (additionalProperties != null),
                AdditionalProperties = additionalProperties
            };

            foreach (var apiProperty in apiObject.ApiProperties)
            {
                // For data annotations, support inline attributes (i.e. applied to member directly) OR attributes applied through a MetadataType
                var memberAttributes = apiProperty.MemberInfo?.GetInlineOrMetadataTypeAttributes() ?? Enumerable.Empty<object>();

                if (Options.IgnoreObsoleteProperties && memberAttributes.OfType<ObsoleteAttribute>().Any())
                    continue;

                schema.Properties.Add(apiProperty.ApiName, GeneratePropertySchema(apiProperty, memberAttributes, schemaRepository));

                if (apiProperty.ApiRequired || memberAttributes.OfType<RequiredAttribute>().Any())
                    schema.Required.Add(apiProperty.ApiName);
            }

            return schema;
        }

        private OpenApiSchema GeneratePropertySchema(
            ApiProperty apiObjectProperty,
            IEnumerable<object> memberAttributes,
            SchemaRepository schemaRepository)
        {
            var schema = Generator.GenerateSchema(apiObjectProperty.Type, schemaRepository);

            // If it's NOT a reference schema, apply contextual metadata (i.e. from MemberInfo)
            if (schema.Reference == null)
            {
                schema.Nullable = apiObjectProperty.ApiNullable;
                schema.ReadOnly = apiObjectProperty.ApiReadOnly;
                schema.WriteOnly = apiObjectProperty.ApiWriteOnly;

                schema.ApplyCustomAttributes(memberAttributes);
            }

            return schema;
        }
    }
}