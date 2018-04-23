using System;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations
{
    public class SwaggerOperationAttributeFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            ApplyOperationAttributes(operation, context);
            ApplyOperationFilterAttributes(operation, context);
        }

        private static void ApplyOperationAttributes(Operation operation, OperationFilterContext context)
        {
            var attribute = context.ApiDescription.ActionAttributes()
                .OfType<SwaggerOperationAttribute>()
                .FirstOrDefault();
            if (attribute == null) return;

            if (attribute.OperationId != null)
                operation.OperationId = attribute.OperationId;

            if (attribute.Tags != null)
                operation.Tags = attribute.Tags;

            if (attribute.Schemes != null)
                operation.Schemes = attribute.Schemes;

            if (attribute.Produces != null)
                operation.Produces = attribute.Produces;

            if (attribute.Consumes != null)
                operation.Consumes = attribute.Consumes;
        }

        public static void ApplyOperationFilterAttributes(Operation operation, OperationFilterContext context)
        {
            var apiDesc = context.ApiDescription;

            var controllerAttributes = apiDesc.ControllerAttributes().OfType<SwaggerOperationFilterAttribute>();
            var actionAttributes = apiDesc.ActionAttributes().OfType<SwaggerOperationFilterAttribute>();

            foreach (var attribute in controllerAttributes.Union(actionAttributes))
            {
                var filter = (IOperationFilter)Activator.CreateInstance(attribute.FilterType);
                filter.Apply(operation, context);
            }
        }
    }
}