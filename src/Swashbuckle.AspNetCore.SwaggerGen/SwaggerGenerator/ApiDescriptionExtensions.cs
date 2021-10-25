using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
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

            // We want to filter out qualifiers that indicate a constraint (":") a default value ("=") or an optional parameter ("?").
            // The filtering should be only done inside of curlies. Outside of them they are part of the normal defined route.

            var result = new StringBuilder();
            var index = 0;
            int nextOpenCurly;
            while ((nextOpenCurly = routeTemplate.IndexOf('{', index)) != -1)
            {
                // append everything before the '{' to the result
                result.Append(routeTemplate.Substring(index, nextOpenCurly - index));
                index = nextOpenCurly;

                var nextClosingCurly = routeTemplate.IndexOf('}', index);
                var contentFromCurly = routeTemplate.Substring(index, nextClosingCurly - index + 1);

                int separatorIndex;
                if ((separatorIndex = contentFromCurly.IndexOf('=')) != -1)
                {
                    // for a default value, take everything after the '='
                    // e.g. "{controller=Home}/{action=Index}" => "{Home}/{Index}"
                    result.Append('{');
                    result.Append(contentFromCurly.Substring(separatorIndex + 1));
                }
                else if ((separatorIndex = contentFromCurly.IndexOf(':')) != -1)
                {
                    // for a contraint, take everything before the constraint
                    // e.g. "collection/{id:int}" => "collection/{id}",
                    result.Append(contentFromCurly.Substring(0, separatorIndex));
                    result.Append('}');
                }
                else if ((separatorIndex = contentFromCurly.IndexOf('?')) != -1)
                {
                    // for an option parameter, just leave out the '?'
                    // e.g. "action/{id?}" => "action/{id}"
                    result.Append(contentFromCurly.Substring(0, separatorIndex));
                    result.Append('}');
                }
                else
                {
                    // there was no separator found, append the whole content
                    result.Append(contentFromCurly);
                }

                index = nextClosingCurly + 1;
            }

            // use everything after the last curlies
            result.Append(routeTemplate.Substring(index));

            return result.ToString();
        }
    }
}
