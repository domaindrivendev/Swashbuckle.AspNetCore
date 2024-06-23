using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing.Template;
using System.Text;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class ApiDescriptionExtensions
    {
        public static bool TryGetMethodInfo(this ApiDescription apiDescription, out MethodInfo methodInfo)
        {
            if (apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                methodInfo = controllerActionDescriptor.MethodInfo;
                return true;
            }

#if NET6_0_OR_GREATER
            if (apiDescription.ActionDescriptor?.EndpointMetadata != null)
            {
                methodInfo = apiDescription.ActionDescriptor.EndpointMetadata
                    .OfType<MethodInfo>()
                    .FirstOrDefault();

                return methodInfo != null;
            }
#endif

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

            return Enumerable.Empty<object>();
        }

        [Obsolete("Use TryGetMethodInfo() and CustomAttributes() instead")]
        public static void GetAdditionalMetadata(this ApiDescription apiDescription,
            out MethodInfo methodInfo,
            out IEnumerable<object> customAttributes)
        {
            if (apiDescription.TryGetMethodInfo(out methodInfo))
            {
                customAttributes = methodInfo.GetCustomAttributes(true)
                    .Union(methodInfo.DeclaringType.GetCustomAttributes(true));

                return;
            }

            customAttributes = Enumerable.Empty<object>();
        }

        internal static string RelativePathSansParameterConstraints(this ApiDescription apiDescription)
        {
            var routeTemplate = apiDescription.RelativePath;

            // remove query string parameters
            var modifiedTemplate = new StringBuilder(routeTemplate.Length);
            var index = 0;
            var isBetweenCurlyBraces = false;
            while (index < routeTemplate.Length)
            {
                var current = routeTemplate[index];

                if (current == '{')
                {
                    isBetweenCurlyBraces = true;
                    goto next;
                }

                if (current == '}')
                {
                    isBetweenCurlyBraces = false;
                    goto next;
                }

                if (!isBetweenCurlyBraces && (current == '?'))
                {
                    break;
                }

            next:
                modifiedTemplate = modifiedTemplate.Append(current);
                index += 1;
            }

            var parsedRouteTemplate = TemplateParser.Parse(modifiedTemplate.ToString());
            var sanitizedSegments = parsedRouteTemplate
                .Segments
                .Select(s => string.Concat(s.Parts.Select(p => p.Name != null ? $"{{{p.Name}}}" : p.Text)));
            return string.Join("/", sanitizedSegments);
        }
    }
}
