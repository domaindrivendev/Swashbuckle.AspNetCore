using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swashbuckle.AspNetCore.Annotations
{
    public class SwaggerTagAttributeDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            var controllersWithTags =
                context.ApiDescriptions
                    .Select(apiDesc => apiDesc.ActionDescriptor)
                    .OfType<ControllerActionDescriptor>()
                    .Select(controllerDescriptor => new
                    {
                        ControllerName = controllerDescriptor.ControllerName,
                        Tags = controllerDescriptor.ControllerTypeInfo.GetCustomAttributes<SwaggerTagAttribute>().Select(attr => attr.Tag)
                    })
                    .Where(controller => controller.Tags.Any())
                    .GroupBy(controller => controller.ControllerName)
                    .ToDictionary(group => group.Key, group => group.First().Tags);

            if (swaggerDoc.Tags == null)
            {
                swaggerDoc.Tags = new List<Tag>();
            }

            foreach (var tag in controllersWithTags.SelectMany(controller => controller.Value))
            {
                swaggerDoc.Tags.Add(tag);
            }
        }
    }
}
