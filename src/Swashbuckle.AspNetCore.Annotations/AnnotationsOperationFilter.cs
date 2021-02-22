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
                Dictionary<string, OpenApiParameter> swaggerHeaderParametersToAdd = new Dictionary<string, OpenApiParameter>();

                foreach (var swaggerHeaderAttribute in swaggerHeaderAttributes)
                {
                    if (!swaggerHeaderParametersToAdd.TryGetValue(swaggerHeaderAttribute.Name, out OpenApiParameter swaggerHeaderParameter))
                    {
                        swaggerHeaderParameter = new OpenApiParameter
                        {
                            In = ParameterLocation.Header,
                            Schema = new OpenApiSchema
                            {
                                Type = "string"
                            }
                        };

                        swaggerHeaderParametersToAdd.Add(swaggerHeaderAttribute.Name, swaggerHeaderParameter);
                    }

                    swaggerHeaderParameter.Name = swaggerHeaderAttribute.Name;
                    swaggerHeaderParameter.Required = swaggerHeaderAttribute.Required;
                    swaggerHeaderParameter.Description = swaggerHeaderAttribute.Description;
                }

                if (operation.Parameters == null)
                {
                    operation.Parameters = new List<OpenApiParameter>(swaggerHeaderParametersToAdd.Values);
                }
                else
                {
                    OpenApiParameter existingHeaderParameter;
                    foreach (var swaggerHeaderParameterToAdd in swaggerHeaderParametersToAdd.Values)
                    {
                        // Honour previously defined headers, outside the scope of the attribute?
                        // If any exist, overwrite them.
                        existingHeaderParameter = operation.Parameters.FirstOrDefault(p => p.Name == swaggerHeaderParameterToAdd.Name && p.In == swaggerHeaderParameterToAdd.In);
                        if (existingHeaderParameter != null)
                        {
                            existingHeaderParameter.Name = swaggerHeaderParameterToAdd.Name;
                            existingHeaderParameter.Required = swaggerHeaderParameterToAdd.Required;
                            existingHeaderParameter.Description = swaggerHeaderParameterToAdd.Description;
                        }
                        else
                        {
                            operation.Parameters.Add(swaggerHeaderParameterToAdd);
                        }
                    }
                }

                swaggerHeaderParametersToAdd.Clear();
            }
        }

        private void ApplySwaggerMultipartFormDataAttributes(OpenApiOperation operation, IEnumerable<object> controllerAndActionAttributes)
        {
            var swaggerMultipartFormDataAttributes = controllerAndActionAttributes.OfType<SwaggerMultiPartFormDataAttribute>();

            if (swaggerMultipartFormDataAttributes.Any())
            {
                var multiPartFormSchema = new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>(),
                    Required = new HashSet<string>()
                };

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
                    multiPartFormMedia = new OpenApiMediaType
                    {
                        Schema = multiPartFormSchema
                    };

                    operation.RequestBody.Content.Add("multipart/form-data", multiPartFormMedia);
                }
                else
                {
                    if (multiPartFormMedia.Schema == null)
                    {
                        multiPartFormMedia.Schema = multiPartFormSchema;
                    }
                    else
                    {
                        // Ensure type is "object"
                        multiPartFormMedia.Schema.Type = "object";

                        if (multiPartFormSchema.Properties == null)
                        {
                            multiPartFormSchema.Properties = new Dictionary<string, OpenApiSchema>();
                        }

                        if (multiPartFormSchema.Required == null)
                        {
                            multiPartFormSchema.Required = new HashSet<string>();
                        }
                    }
                }

                foreach (var swaggerMultipartFormDataAttribute in swaggerMultipartFormDataAttributes)
                {
                    if (!multiPartFormSchema.Properties.TryGetValue(swaggerMultipartFormDataAttribute.Name, out OpenApiSchema schemaPropertyToAdd))
                    {
                        schemaPropertyToAdd = new OpenApiSchema();

                        multiPartFormSchema.Properties.Add(swaggerMultipartFormDataAttribute.Name, schemaPropertyToAdd);
                    }

                    schemaPropertyToAdd.Type = swaggerMultipartFormDataAttribute.Type ?? "file";
                    schemaPropertyToAdd.Format = swaggerMultipartFormDataAttribute.Type == null ? "binary" : swaggerMultipartFormDataAttribute.Type;

                    if (swaggerMultipartFormDataAttribute.Required)
                    {
                        multiPartFormSchema.Required.Add(swaggerMultipartFormDataAttribute.Name);
                    }
                    else
                    {
                        multiPartFormSchema.Required.Remove(swaggerMultipartFormDataAttribute.Name);
                    }
                }
            }
        }
    }
}
