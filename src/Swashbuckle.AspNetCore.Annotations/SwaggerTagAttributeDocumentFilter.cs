using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using System.Reflection;

namespace Swashbuckle.AspNetCore.Annotations
{
    public class SwaggerTagAttributeDocumentFilter : IDocumentFilter
    {
        private static Type swaggerTagType = typeof(SwaggerTagAttribute);

        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            var controllerNamesAndTagAttributes = context.ApiDescriptions
                    .Select(apiDesc => apiDesc.ActionDescriptor as ControllerActionDescriptor)
                    .SkipWhile(actionDesc => !HasSwaggerTags(actionDesc))
                    .GroupBy(actionDesc => actionDesc.ControllerName)
                    .ToDictionary(grp => grp.Key, grp => grp.Last().ControllerTypeInfo
                        .GetCustomAttributes<SwaggerTagAttribute>()
                        .Select(attr => attr.Tag)
                        .ToList());

            foreach (var tag in controllerNamesAndTagAttributes.SelectMany(controller => controller.Value))
            {
                swaggerDoc.Tags.Add(tag);
            }
        }

        private static bool HasSwaggerTags(ActionDescriptor actionDescription)
        {
            if (actionDescription is ControllerActionDescriptor controllerDescriptor)
            {
                return controllerDescriptor.ControllerTypeInfo.CustomAttributes.Any(t => t.AttributeType == swaggerTagType);
            }

            return false;
        }
        
    }
}
