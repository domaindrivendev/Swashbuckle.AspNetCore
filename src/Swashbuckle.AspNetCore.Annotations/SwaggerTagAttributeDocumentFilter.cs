using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// A document filter that creates Tags on the Document (Swagger object),
    /// defined by the <see cref="SwaggerTagAttribute" /> on controllers.
    /// </summary>
    /// <remarks>
    /// This filter does not alter the tags of the operations where the attribute is applied.
    /// Neither does it ensure that you have unique tags, so if you tag two controllers with the same tag but two different descriptions the result is that multiple tags with different descriptions are added.
    /// </remarks>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.IDocumentFilter" />
    public class SwaggerTagAttributeDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            var controllersWithTags = GetControllersAndTags(context);
            EnsureDocumentTagsList(swaggerDoc);
            ApplyTags(swaggerDoc, controllersWithTags);
        }

        private static void ApplyTags(SwaggerDocument swaggerDoc, Dictionary<string, IEnumerable<Tag>> controllersWithTags)
        {
            foreach (var tag in GetValidTags(controllersWithTags))
            {
                swaggerDoc.Tags.Add(tag);
            }
        }

        private static Dictionary<string, IEnumerable<Tag>> GetControllersAndTags(DocumentFilterContext context)
        {
            return context.ApiDescriptions
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
        }

        private static void EnsureDocumentTagsList(SwaggerDocument swaggerDoc)
        {
            if (swaggerDoc.Tags == null)
            {
                swaggerDoc.Tags = new List<Tag>();
            }
        }

        private static IEnumerable<Tag> GetValidTags(Dictionary<string, IEnumerable<Tag>> controllersWithTags)
        {
            return controllersWithTags
                .SelectMany(controller => controller.Value)
                .Where(tag => !string.IsNullOrWhiteSpace(tag.Name));
        }
    }
}
