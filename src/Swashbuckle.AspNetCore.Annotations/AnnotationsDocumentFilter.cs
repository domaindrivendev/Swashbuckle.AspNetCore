using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations;

public class AnnotationsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags ??= [];

        // Collect (unique) controller names and custom attributes in a dictionary
        var controllerNamesAndAttributes = context.ApiDescriptions
            .Select(apiDesc => apiDesc.ActionDescriptor as ControllerActionDescriptor)
            .Where(actionDesc => actionDesc != null)
            .GroupBy(actionDesc => actionDesc.ControllerName)
            .Select(group => new KeyValuePair<string, IEnumerable<object>>(group.Key, group.First().ControllerTypeInfo.GetCustomAttributes(true)));

        foreach (var entry in controllerNamesAndAttributes)
        {
            ApplySwaggerTagAttribute(swaggerDoc, entry.Key, entry.Value);
        }
    }

    private static void ApplySwaggerTagAttribute(
        OpenApiDocument document,
        string controllerName,
        IEnumerable<object> customAttributes)
    {
        var swaggerTagAttribute = customAttributes
            .OfType<SwaggerTagAttribute>()
            .FirstOrDefault();

        if (swaggerTagAttribute is null)
        {
            return;
        }

        var tag = document.Tags.FirstOrDefault((p) => p?.Name == controllerName);

        if (tag is null)
        {
            tag = new() { Name = controllerName };
            document.Tags.Add(tag);
        }

        tag.Description ??= swaggerTagAttribute.Description;

        if (swaggerTagAttribute.ExternalDocsUrl is { } url)
        {
            tag.ExternalDocs ??= new OpenApiExternalDocs { Url = new(url) };
        }
    }
}
