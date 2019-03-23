using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations
{
    public class AnnotationsDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc.Tags == null)
                swaggerDoc.Tags = new List<OpenApiTag>();

            // Collect (unique) controller names and custom attributes in a dictionary
            var controllerNamesAndAttributes = context.ApiDescriptions
                .Select(apiDesc => apiDesc.ActionDescriptor as ControllerActionDescriptor)
                .SkipWhile(actionDesc => actionDesc == null)
                .GroupBy(actionDesc => actionDesc.ControllerName)
                .Select(group => new KeyValuePair<string, IEnumerable<object>>(group.Key, group.First().ControllerTypeInfo.GetCustomAttributes(true)));

            foreach (var entry in controllerNamesAndAttributes)
            {
                ApplySwaggerTagAttribute(swaggerDoc, entry.Key, entry.Value);
            }
        }

        private void ApplySwaggerTagAttribute(
            OpenApiDocument swaggerDoc,
            string controllerName,
            IEnumerable<object> customAttributes)
        {
            var swaggerTagAttribute = customAttributes
                .OfType<SwaggerTagAttribute>()
                .FirstOrDefault();

            if (swaggerTagAttribute == null) return;

            swaggerDoc.Tags.Add(new OpenApiTag
            {
                Name = controllerName,
                Description = swaggerTagAttribute.Description,
                ExternalDocs = (swaggerTagAttribute.ExternalDocsUrl != null)
                    ? new OpenApiExternalDocs { Url = new Uri(swaggerTagAttribute.ExternalDocsUrl) }
                    : null
            });
        }
    }
}
