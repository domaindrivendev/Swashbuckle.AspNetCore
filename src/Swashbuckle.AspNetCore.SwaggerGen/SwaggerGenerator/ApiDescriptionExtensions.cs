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

            // We want to filter out qualifiers that indicate a constract (":")
            // a default value ("=") or an optional parameter ("?")

            // Those qualifiers are actually ok to be part of the normal route, outside of a parameter.
            // To handle this, we're splitting the route into parts where everything outside of a parameter
            // is a part and the part itself as well. Then the filtering will only by executed on the parts of
            // the parameters.

            var depth = 0;
            var lastPartStart = 0;
            var parts = new List<string>();
            for (int i = 0; i < routeTemplate.Length; i++)
            {
                if (routeTemplate[i] == '{')
                {
                    if (depth == 0)
                    {
                        if (i - lastPartStart > 0)
                        {
                            // if there is a non empty part, add it.
                            parts.Add(routeTemplate.Substring(lastPartStart, i - lastPartStart));
                        }

                        lastPartStart = i;
                    }

                    depth++;
                }
                else if (routeTemplate[i] == '}')
                {
                    depth--;

                    if (depth == 0)
                    {
                        // add parameter part including closing curly
                        parts.Add(routeTemplate.Substring(lastPartStart, i - lastPartStart + 1));

                        // set lastPartStart to next character to not include the closing curly.
                        lastPartStart = i + 1;
                    }
                }
            }

            // add remainder if it exists
            if (lastPartStart < routeTemplate.Length)
            {
                parts.Add(routeTemplate.Substring(lastPartStart));
            }

            var result = new StringBuilder();

            foreach (var part in parts)
            {
                var filteredPart = part;

                if (filteredPart.StartsWith("{"))
                {
                    while (filteredPart.IndexOfAny(new[] { ':', '=', '?' }) != -1)
                    {
                        var startIndex = filteredPart.IndexOfAny(new[] { ':', '=', '?' });
                        var tokenStart = startIndex + 1;
                    // Only find a single instance of a '}' after our start
                    // character to avoid capturing escaped curly braces
                    // in a regular expression constraint
                    findEndBrace:
                        var endIndex = filteredPart.IndexOf('}', tokenStart);
                        if (endIndex < filteredPart.Length - 1 && filteredPart[endIndex + 1] == '}')
                        {
                            tokenStart = endIndex + 2;
                            goto findEndBrace;
                        }

                        filteredPart = filteredPart.Remove(startIndex, endIndex - startIndex);

                    }
                }

                result.Append(filteredPart);
            }

            return result.ToString();
        }
    }
}
