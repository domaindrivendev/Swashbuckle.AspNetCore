using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class ApiDescriptionExtensions
    {
        private static readonly char[] Disallowed = { ':', '=', '?' };

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

            // We want to filter out qualifiers that indicate a constraint (":")
            // a default value ("=") or an optional parameter ("?")
            if (routeTemplate.IndexOfAny(Disallowed) == -1)
            {
                // Quick-exit if there's nothing to remove.
                return routeTemplate;
            }

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

                if (isBetweenCurlyBraces && Disallowed.Contains(current))
                {
                    // Only find a single instance of a '}' after our start
                    // character to avoid capturing escaped curly braces
                    // in a regular expression constraint
                findEndBrace:
                    index = routeTemplate.IndexOf('}', index + 1);
                    if (index < routeTemplate.Length - 1 && routeTemplate[index + 1] == '}')
                    {
                        index += 2;
                        goto findEndBrace;
                    }
                    continue;
                }

            next:
                modifiedTemplate = modifiedTemplate.Append(current);
                index += 1;
            }

            return modifiedTemplate.ToString();
        }
    }
}
