using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

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
            var attributes = context.Type.GetCustomAttributes<SwaggerSchemaFilterAttribute>(true);

            foreach (var attr in attributes)
            {
                var filter = (ISchemaFilter)ActivatorUtilities.CreateInstance(_serviceProvider, attr.Type, attr.Arguments);
                filter.Apply(schema, context);
            }
        }
    }
}
