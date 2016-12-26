using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SwaggerAttributesSchemaFilter : ISchemaFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public SwaggerAttributesSchemaFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Apply(Schema schema, SchemaFilterContext context)
        {
            var typeInfo = context.SystemType.GetTypeInfo();
            var attributes = typeInfo.GetCustomAttributes(false).OfType<SwaggerSchemaFilterAttribute>();

            foreach (var attr in attributes)
            {
                var filter = (ISchemaFilter)ActivatorUtilities.CreateInstance(_serviceProvider, attr.Type, attr.Arguments);
                filter.Apply(schema, context);
            }
        }
    }
}
