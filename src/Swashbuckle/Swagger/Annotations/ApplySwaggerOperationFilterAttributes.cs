using System;
using System.Linq;

namespace Swashbuckle.Swagger.Annotations
{
    public class ApplySwaggerOperationFilterAttributes : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var apiDesc = context.ApiDescription;

            var controllerAttributes = apiDesc.GetControllerAttributes().OfType<SwaggerOperationFilterAttribute>();
            var actionAttributes = apiDesc.GetActionAttributes().OfType<SwaggerOperationFilterAttribute>();

            foreach (var attribute in controllerAttributes.Union(actionAttributes))
            {
                var filter = (IOperationFilter)Activator.CreateInstance(attribute.FilterType);
                filter.Apply(operation, context);
            }
        }
    }
}
