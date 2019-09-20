using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations
{
    public class AnnotationsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo == null) return;

            // Action descriptions should take precedence over controller descriptions and
            // the most derived controller descrption should take precedence over the least,
            // so do not overwrite the description if there is a value already there.
            var controllerAttributes = context.MethodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes(true);
            var actionAttributes = context.MethodInfo.GetCustomAttributes(true);
            var controllerAndActionAttributes = controllerAttributes.Union(actionAttributes);

            ApplySwaggerOperationAttribute(operation, actionAttributes);
            ApplySwaggerOperationFilterAttributes(operation, context, controllerAndActionAttributes);
            ApplySwaggerResponseAttributes(operation, controllerAndActionAttributes);
        }

        private static void ApplySwaggerOperationAttribute(
            OpenApiOperation operation,
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
            {
                operation.Tags = swaggerOperationAttribute.Tags
                    .Select(tagName => new OpenApiTag { Name = tagName })
                    .ToList();
            }
        }

        public static void ApplySwaggerOperationFilterAttributes(
            OpenApiOperation operation,
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
            OpenApiOperation operation,
            IEnumerable<object> actionAndControllerAttributes)
        {
            var swaggerResponseAttributes = actionAndControllerAttributes
                .OfType<SwaggerResponseAttribute>();

            foreach (var swaggerResponseAttribute in swaggerResponseAttributes)
            {
                var statusCode = swaggerResponseAttribute.StatusCode.ToString();

                if (operation.Responses == null)
                {
                    operation.Responses = new OpenApiResponses();
                }

                if (!operation.Responses.TryGetValue(statusCode, out OpenApiResponse response))
                {
                    response = new OpenApiResponse();
                }

                if (swaggerResponseAttribute.Description != null)
                    response.Description = swaggerResponseAttribute.Description;

                operation.Responses[statusCode] = response;
            }
        }
    }
}
