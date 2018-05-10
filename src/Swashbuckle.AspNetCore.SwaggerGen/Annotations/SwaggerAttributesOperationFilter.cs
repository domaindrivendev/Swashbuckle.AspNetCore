using System;
using System.Linq;
using System.Reflection;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SwaggerAttributesOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.ControllerActionDescriptor == null) return;

            ApplyOperationAttributes(operation, context);
            ApplyOperationFilterAttributes(operation, context);
        }

        private static void ApplyOperationAttributes(Operation operation, OperationFilterContext context)
        {
            var swaggerOperationAttribute = context.ControllerActionDescriptor.MethodInfo
                .GetCustomAttributes(true)
                .OfType<SwaggerOperationAttribute>()
                .FirstOrDefault();

            if (swaggerOperationAttribute == null) return;

            if (swaggerOperationAttribute.OperationId != null)
                operation.OperationId = swaggerOperationAttribute.OperationId;

            if (swaggerOperationAttribute.Tags != null)
                operation.Tags = swaggerOperationAttribute.Tags;

            if (swaggerOperationAttribute.Schemes != null)
                operation.Schemes = swaggerOperationAttribute.Schemes;
        }

        public static void ApplyOperationFilterAttributes(Operation operation, OperationFilterContext context)
        {
            var swaggerOperationFilterAttributes = context.ControllerActionDescriptor
                .GetControllerAndActionAttributes(true)
                .OfType<SwaggerOperationFilterAttribute>();

            foreach (var swaggerOperationFilterAttribute in swaggerOperationFilterAttributes)
            {
                var filter = (IOperationFilter)Activator.CreateInstance(swaggerOperationFilterAttribute.FilterType);
                filter.Apply(operation, context);
            }
        }
    }
}