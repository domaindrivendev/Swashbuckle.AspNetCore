using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using System.Reflection;

namespace Swashbuckle.AspNetCore.Annotations
{
    public class AnnotationsSchemaFilter : ISchemaFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public AnnotationsSchemaFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            ApplySwaggerSchemaFilterAttribute(schema, context);
            ApplySwaggerPropertyAttribute(schema, context);
        }

        private void ApplySwaggerSchemaFilterAttribute(OpenApiSchema schema, SchemaFilterContext context)
        {
            var schemaFilterAttributes = context.ApiModel.Type.GetCustomAttributes<SwaggerSchemaFilterAttribute>();

            foreach (var attr in schemaFilterAttributes)
            {
                var filter = (ISchemaFilter)ActivatorUtilities.CreateInstance(_serviceProvider, attr.Type, attr.Arguments);
                filter.Apply(schema, context);
            }
        }

        private void ApplySwaggerPropertyAttribute(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties.Any())
            {
                foreach (var prop in context.ApiModel.Type.GetProperties())
                {
                    var propAttributeObj = prop.GetCustomAttributes(true).FirstOrDefault(p => p.GetType() == typeof(SwaggerPropertyAttribute));
                    if (propAttributeObj != null)
                    {
                        var propAttribute = propAttributeObj as SwaggerPropertyAttribute;

                        if (context.SchemaRepository.Schemas.Count(p => p.Value.Reference.Id == schema.Properties[prop.Name].Reference.Id) < 2)
                            context.SchemaRepository.Schemas.Remove(schema.Properties[prop.Name].Reference.Id);

                        schema.Properties[prop.Name] = context.SchemaGenerator.GenerateSchema(propAttribute.OverridedType, context.SchemaRepository);
                    }
                }
            }
            else if(schema.Reference.IsLocal)
            {
                foreach (var prop in context.ApiModel.Type.GetProperties())
                {
                    var propAttributeObj = prop.GetCustomAttributes(true).FirstOrDefault(p => p.GetType() == typeof(SwaggerPropertyAttribute));
                    if (propAttributeObj != null)
                    {
                        var propAttribute = propAttributeObj as SwaggerPropertyAttribute;
                        var schemaProp = context.SchemaRepository.Schemas.First(p => p.Key == schema.Reference.Id).Value.Properties.First(p => p.Key == prop.Name);
                        if (context.SchemaRepository.Schemas.Count(p => p.Value?.Reference?.Id == schemaProp.Value.Reference.Id) < 2)
                            context.SchemaRepository.Schemas.Remove(schemaProp.Value.Reference.Id);

                        schema.Properties[prop.Name] = context.SchemaGenerator.GenerateSchema(propAttribute.OverridedType, context.SchemaRepository);
                    }
                }
            }
        }
    }
}
