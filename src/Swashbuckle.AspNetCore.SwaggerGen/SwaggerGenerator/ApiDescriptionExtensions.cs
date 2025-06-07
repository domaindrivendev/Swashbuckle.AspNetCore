using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing.Template;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public static class ApiDescriptionExtensions
{
    public static bool TryGetMethodInfo(this ApiDescription apiDescription, out MethodInfo methodInfo)
    {
        if (apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            methodInfo = controllerActionDescriptor.MethodInfo;
            return true;
        }

        if (apiDescription.ActionDescriptor?.EndpointMetadata != null)
        {
            methodInfo = apiDescription.ActionDescriptor.EndpointMetadata
                .OfType<MethodInfo>()
                .FirstOrDefault();

            return methodInfo != null;
        }

        methodInfo = null;
        return false;
    }

    public static IEnumerable<object> CustomAttributes(this ApiDescription apiDescription)
    {
        if (apiDescription.TryGetMethodInfo(out MethodInfo methodInfo))
        {
            return methodInfo.GetCustomAttributes(true)
                .Union(methodInfo.DeclaringType.GetCustomAttributes(true));
        }

        return [];
    }

    internal static string RelativePathSansParameterConstraints(this ApiDescription apiDescription)
    {
        var routeTemplate = TemplateParser.Parse(apiDescription.RelativePath);
        var sanitizedSegments = routeTemplate
            .Segments
            .Select(s => string.Concat(s.Parts.Select(p => p.Name != null ? $"{{{p.Name}}}" : p.Text)));

        return string.Join('/', sanitizedSegments);
    }
}
