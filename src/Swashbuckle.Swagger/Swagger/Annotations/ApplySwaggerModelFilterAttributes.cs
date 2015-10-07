using System;
using System.Linq;
using System.Reflection;

namespace Swashbuckle.Swagger.Annotations
{
    public class ApplySwaggerModelFilterAttributes : IModelFilter
    {
        public void Apply(Schema model, ModelFilterContext context)
        {
            var typeInfo = context.SystemType.GetTypeInfo();
            var attributes = typeInfo.GetCustomAttributes(false).OfType<SwaggerModelFilterAttribute>();

            foreach (var attribute in attributes)
            {
                var filter = (IModelFilter)Activator.CreateInstance(attribute.FilterType);
                filter.Apply(model, context);
            }
        }
    }
}
