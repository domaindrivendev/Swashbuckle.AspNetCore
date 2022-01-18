using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations
{
    public class AnnotationsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            IEnumerable<object> controllerAttributes = Array.Empty<object>();
            IEnumerable<object> actionAttributes = Array.Empty<object>();
            IEnumerable<object> metadataAttributes = Array.Empty<object>();

            if (context.MethodInfo != null)
            {
                controllerAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true);
                actionAttributes = context.MethodInfo.GetCustomAttributes(true);
            }

#if NET6_0_OR_GREATER
            if (context.ApiDescription?.ActionDescriptor?.EndpointMetadata != null)
            {
                metadataAttributes = context.ApiDescription.ActionDescriptor.EndpointMetadata;
            }
#endif

            // NOTE: When controller and action attributes are applicable, action attributes should take precendence.
            // Hence why they're at the end of the list (i.e. last one wins).
            // Distinct() is applied due to an ASP.NET Core issue: https://github.com/dotnet/aspnetcore/issues/34199.
            var allAttributes = controllerAttributes
                .Union(actionAttributes)
                .Union(metadataAttributes)
                .Distinct();

            var actionAndEndpointAttribtues = actionAttributes
                .Union(metadataAttributes)
                .Distinct();

            ApplySwaggerOperationAttribute(operation, actionAndEndpointAttribtues);
            ApplySwaggerOperationFilterAttributes(operation, context, allAttributes);
            ApplySwaggerResponseAttributes(operation, context, allAttributes);
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
            IEnumerable<object> controllerAndActionAttributes)
        {
            var swaggerOperationFilterAttributes = controllerAndActionAttributes
                .OfType<SwaggerOperationFilterAttribute>();

            foreach (var swaggerOperationFilterAttribute in swaggerOperationFilterAttributes)
            {
                var filter = (IOperationFilter)Activator.CreateInstance(swaggerOperationFilterAttribute.FilterType);
                filter.Apply(operation, context);
            }
        }

        private void ApplySwaggerResponseAttributes(
            OpenApiOperation operation,
            OperationFilterContext context,
            IEnumerable<object> controllerAndActionAttributes)
        {
            var swaggerResponseAttributes = controllerAndActionAttributes.OfType<SwaggerResponseAttribute>();

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
                {
                    response.Description = swaggerResponseAttribute.Description;
                }

                operation.Responses[statusCode] = response;

                if (swaggerResponseAttribute.ContentTypes != null)
                {
                    response.Content.Clear();

                    foreach (var contentType in swaggerResponseAttribute.ContentTypes)
                    {
                        var schema = (swaggerResponseAttribute.Type != null && swaggerResponseAttribute.Type != typeof(void))
                            ? context.SchemaGenerator.GenerateSchema(swaggerResponseAttribute.Type, context.SchemaRepository)
                            : null;

                        response.Content.Add(contentType, new OpenApiMediaType { Schema = schema });
                    }
                }
            }
        }
    }
}
