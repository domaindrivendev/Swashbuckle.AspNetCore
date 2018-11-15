using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations
{
    public class AnnotationsOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.MethodInfo == null) return;

            var actionAttributes = context.MethodInfo.GetCustomAttributes(true);
            var controllerAttributes = context.MethodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes(true);
            var actionAndControllerAttributes = actionAttributes.Union(controllerAttributes);

            ApplySwaggerOperationAttribute(operation, actionAttributes);
            ApplySwaggerOperationFilterAttributes(operation, context, actionAndControllerAttributes);
            ApplySwaggerResponseAttributes(operation, actionAndControllerAttributes, context);
        }

        private static void ApplySwaggerOperationAttribute(
            Operation operation,
            IEnumerable<object> actionAttributes)
        {
            var swaggerOperationAttribute = actionAttributes
                .OfType<SwaggerOperationAttribute>()
                .FirstOrDefault();

            if (swaggerOperationAttribute == null) return;

            if (swaggerOperationAttribute.Summary != null)
                operation.Summary = swaggerOperationAttribute.Summary;

            if (swaggerOperationAttribute.Description != null)
                operation.Description = swaggerOperationAttribute.Description;

            if (swaggerOperationAttribute.OperationId != null)
                operation.OperationId = swaggerOperationAttribute.OperationId;

            if (swaggerOperationAttribute.Tags != null)
                operation.Tags = swaggerOperationAttribute.Tags;

            if (swaggerOperationAttribute.Consumes != null)
                operation.Consumes = swaggerOperationAttribute.Consumes;

            if (swaggerOperationAttribute.Produces != null)
                operation.Produces = swaggerOperationAttribute.Produces;

            if (swaggerOperationAttribute.Schemes != null)
                operation.Schemes = swaggerOperationAttribute.Schemes;
        }

        public static void ApplySwaggerOperationFilterAttributes(
            Operation operation,
            OperationFilterContext context,
            IEnumerable<object> actionAndControllerAttributes)
        {
            var swaggerOperationFilterAttributes = actionAndControllerAttributes
                .OfType<SwaggerOperationFilterAttribute>();

            foreach (var swaggerOperationFilterAttribute in swaggerOperationFilterAttributes)
            {
                var filter = (IOperationFilter)Activator.CreateInstance(swaggerOperationFilterAttribute.FilterType);
                filter.Apply(operation, context);
            }
        }

        private void ApplySwaggerResponseAttributes(
            Operation operation,
            IEnumerable<object> actionAndControllerAttributes,
            OperationFilterContext context)
        {
            var swaggerResponseAttributes = actionAndControllerAttributes
                .OfType<SwaggerResponseAttribute>();

            foreach (var swaggerResponseAttribute in swaggerResponseAttributes)
            {
                var statusCode = swaggerResponseAttribute.StatusCode.ToString();
                if (!operation.Responses.TryGetValue(statusCode, out Response response))
                {
                    response = new Response();
                }

                if (swaggerResponseAttribute.Description != null)
                    response.Description = swaggerResponseAttribute.Description;

                if (swaggerResponseAttribute.Type != null)
                    response.Schema = context.SchemaRegistry.GetOrRegister(swaggerResponseAttribute.Type);

                operation.Responses[statusCode] = response;
            }
        }
    }
}
