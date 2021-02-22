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
            if (context.MethodInfo == null) return;

            var controllerAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true);
            var actionAttributes = context.MethodInfo.GetCustomAttributes(true);

            // NOTE: When controller and action attributes are applicable, action attributes should take precendence.
            // Hence why they're at the end of the list (i.e. last one wins)
            var controllerAndActionAttributes = controllerAttributes.Union(actionAttributes);

            ApplySwaggerOperationAttribute(operation, actionAttributes);
            ApplySwaggerOperationFilterAttributes(operation, context, controllerAndActionAttributes);
            ApplySwaggerResponseAttributes(operation, controllerAndActionAttributes);
            ApplySwaggerHeaderAttributes(operation, controllerAndActionAttributes);
            ApplySwaggerMultipartFormDataAttributes(operation, controllerAndActionAttributes);
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
            IEnumerable<object> controllerAndActionAttributes)
        {
            var swaggerResponseAttributes = controllerAndActionAttributes
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

        private void ApplySwaggerHeaderAttributes(OpenApiOperation operation,
            IEnumerable<object> controllerAndActionAttributes)
        {
            var swaggerHeaderAttributes = controllerAndActionAttributes.OfType<SwaggerHeaderAttribute>();

            if (swaggerHeaderAttributes.Any())
            {
                if (operation.Parameters == null)
                {
                    operation.Parameters = new List<OpenApiParameter>();
                }

                OpenApiParameter headerParameter;
                foreach (var swaggerHeaderAttribute in swaggerHeaderAttributes)
                {
                    // Honor previously defined headers, outside the scope of the attribute?
                    // If any exist, overwrite them.
                    headerParameter = operation.Parameters.FirstOrDefault(p => p.Name == swaggerHeaderAttribute.Name && p.In == ParameterLocation.Header);
                    if (headerParameter == null)
                    {
                        headerParameter = new OpenApiParameter
                        {
                            In = ParameterLocation.Header
                        };

                        operation.Parameters.Add(headerParameter);
                    }

                    headerParameter.Name = swaggerHeaderAttribute.Name;
                    headerParameter.Required = swaggerHeaderAttribute.Required;
                    headerParameter.Description = swaggerHeaderAttribute.Description;

                    if (headerParameter.Schema == null)
                    {
                        headerParameter.Schema = new OpenApiSchema();
                    }

                    headerParameter.Schema.Type = swaggerHeaderAttribute.Type ?? "string";
                    headerParameter.Schema.Format = swaggerHeaderAttribute.Type is null ? null : swaggerHeaderAttribute.Format;
                }
            }
        }

        private void ApplySwaggerMultipartFormDataAttributes(OpenApiOperation operation, IEnumerable<object> controllerAndActionAttributes)
        {
            var swaggerMultipartFormDataAttributes = controllerAndActionAttributes.OfType<SwaggerMultiPartFormDataAttribute>();

            if (swaggerMultipartFormDataAttributes.Any())
            {
                if (operation.RequestBody == null)
                {
                    operation.RequestBody = new OpenApiRequestBody();
                }

                if (operation.RequestBody.Content == null)
                {
                    operation.RequestBody.Content = new Dictionary<string, OpenApiMediaType>();
                }

                if (!operation.RequestBody.Content.TryGetValue("multipart/form-data", out OpenApiMediaType multiPartFormMedia))
                {
                    multiPartFormMedia = new OpenApiMediaType();

                    operation.RequestBody.Content.Add("multipart/form-data", multiPartFormMedia);
                }

                if (multiPartFormMedia.Schema == null)
                {
                    multiPartFormMedia.Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>(),
                        Required = new HashSet<string>()
                    };
                }
                else
                {
                    // Ensure type is "object"
                    multiPartFormMedia.Schema.Type = "object";

                    if (multiPartFormMedia.Schema.Properties == null)
                    {
                        multiPartFormMedia.Schema.Properties = new Dictionary<string, OpenApiSchema>();
                    }

                    if (multiPartFormMedia.Schema.Required == null)
                    {
                        multiPartFormMedia.Schema.Required = new HashSet<string>();
                    }
                }

                foreach (var swaggerMultipartFormDataAttribute in swaggerMultipartFormDataAttributes)
                {
                    // Honor previously defined headers, outside the scope of the attribute?
                    // If any exist, overwrite them.
                    if (!multiPartFormMedia.Schema.Properties.TryGetValue(swaggerMultipartFormDataAttribute.Name, out OpenApiSchema schemaProperty))
                    {
                        schemaProperty = new OpenApiSchema();

                        multiPartFormMedia.Schema.Properties.Add(swaggerMultipartFormDataAttribute.Name, schemaProperty);
                    }

                    schemaProperty.Type = swaggerMultipartFormDataAttribute.Type ?? "file";
                    schemaProperty.Format = swaggerMultipartFormDataAttribute.Type == null ? "binary" : swaggerMultipartFormDataAttribute.Format;

                    if (swaggerMultipartFormDataAttribute.Required)
                    {
                        multiPartFormMedia.Schema.Required.Add(swaggerMultipartFormDataAttribute.Name);
                    }
                    else
                    {
                        multiPartFormMedia.Schema.Required.Remove(swaggerMultipartFormDataAttribute.Name);
                    }
                }
            }
        }
    }
}
