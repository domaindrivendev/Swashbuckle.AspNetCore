using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations
{
    public class SwaggerOperationAttributeFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
           if (context.MethodInfo == null) return;

           var actionAttributes = context.MethodInfo.GetCustomAttributes(true);
           var controllerAttributes = context.MethodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes(true);

            ApplyOperationAttributes(operation, actionAttributes);
            ApplyOperationFilterAttributes(operation, actionAttributes, controllerAttributes, context);
        }

        private static void ApplyOperationAttributes(Operation operation, IEnumerable<object> actionAttributes)
        {
            var swaggerOperationAttribute = actionAttributes
                .OfType<SwaggerOperationAttribute>()
                .FirstOrDefault();

            if (swaggerOperationAttribute == null) return;

            if (swaggerOperationAttribute.OperationId != null)
                operation.OperationId = swaggerOperationAttribute.OperationId;

            if (swaggerOperationAttribute.Tags != null)
                operation.Tags = swaggerOperationAttribute.Tags;

            if (swaggerOperationAttribute.Schemes != null)
                operation.Schemes = swaggerOperationAttribute.Schemes;

            if (swaggerOperationAttribute.Produces != null)
                operation.Produces = swaggerOperationAttribute.Produces;

            if (swaggerOperationAttribute.Consumes != null)
                operation.Consumes = swaggerOperationAttribute.Consumes;
        }

        public static void ApplyOperationFilterAttributes(
            Operation operation,
            IEnumerable<object> actionAttributes,
            IEnumerable<object> controllerAttributes,
            OperationFilterContext context)
        {
            var swaggerOperationFilterAttributes = actionAttributes.Union(controllerAttributes)
                .OfType<SwaggerOperationFilterAttribute>();

            foreach (var swaggerOperationFilterAttribute in swaggerOperationFilterAttributes)
            {
                var filter = (IOperationFilter)Activator.CreateInstance(swaggerOperationFilterAttribute.FilterType);
                filter.Apply(operation, context);
            }
        }
    }
}