using Microsoft.OpenApi.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class DescriptionAttributeSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            ApplyTypeTags(schema, context.Type);

            if (context.MemberInfo != null)
            {
                ApplyFieldOrPropertyTags(schema, context.MemberInfo);
            }
        }

        private void ApplyFieldOrPropertyTags(OpenApiSchema schema, MemberInfo memberInfo)
        {
            string description = memberInfo.CustomAttributes.Where(x => typeof(DescriptionAttribute) == x.AttributeType).FirstOrDefault()?.ConstructorArguments.FirstOrDefault().Value?.ToString();

            schema.Description = description != null ? description : schema.Description;
        }

        private void ApplyTypeTags(OpenApiSchema schema, Type type)
        {
            var description = type.CustomAttributes.Where(x => typeof(DescriptionAttribute) == x.AttributeType).FirstOrDefault()?.ConstructorArguments.FirstOrDefault().Value?.ToString();

            schema.Description = description != null ? description : schema.Description;
        }
    }
}
