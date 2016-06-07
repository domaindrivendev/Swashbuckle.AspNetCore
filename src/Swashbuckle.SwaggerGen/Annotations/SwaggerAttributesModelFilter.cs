using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;

namespace Swashbuckle.SwaggerGen.Annotations
{
    public class SwaggerAttributesModelFilter : IModelFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public SwaggerAttributesModelFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Apply(Schema model, ModelFilterContext context)
        {
            var typeInfo = context.SystemType.GetTypeInfo();
            var attributes = typeInfo.GetCustomAttributes(false).OfType<SwaggerModelFilterAttribute>();

            foreach (var attr in attributes)
            {
                var filter = (IModelFilter)ActivatorUtilities.CreateInstance(_serviceProvider, attr.Type, attr.Arguments);
                filter.Apply(model, context);
            }
        }
    }
}
