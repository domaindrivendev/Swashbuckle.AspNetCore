using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations
{
    public class AnnotationsDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc.Tags == null)
                swaggerDoc.Tags = new List<Tag>();

            // Collect (unique) controller names and custom attributes in a dictionary
            var controllerNamesAndAttributes = context.ApiDescriptions
                .Select(apiDesc => apiDesc.ActionDescriptor as ControllerActionDescriptor)
                .SkipWhile(actionDesc => actionDesc == null)
                .GroupBy(actionDesc => actionDesc.ControllerName)
                .ToDictionary(grp => grp.Key, grp => grp.Last().ControllerTypeInfo.GetCustomAttributes(true));

            foreach (var entry in controllerNamesAndAttributes)
            {
                ApplySwaggerTagAttribute(swaggerDoc, entry.Key, entry.Value);
            }
        }

        private void ApplySwaggerTagAttribute(
            SwaggerDocument swaggerDoc,
            string controllerName,
            IEnumerable<object> customAttributes)
        {
            var swaggerTagAttribute = customAttributes
                .OfType<SwaggerTagAttribute>()
                .FirstOrDefault();

            if (swaggerTagAttribute == null) return;

            swaggerDoc.Tags.Add(new Tag
            {
                Name = controllerName,
                Description = swaggerTagAttribute.Description,
                ExternalDocs = (swaggerTagAttribute.ExternalDocsUrl != null)
                    ? new ExternalDocs { Url = swaggerTagAttribute.ExternalDocsUrl }
                    : null
            });
        }
    }
}
