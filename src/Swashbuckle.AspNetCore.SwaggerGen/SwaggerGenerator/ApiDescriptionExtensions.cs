﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;

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
            while (routeTemplate.IndexOfAny(new[] { ':', '=', '?' }) != -1)
            {
                var startIndex = routeTemplate.IndexOfAny(new[] { ':', '=', '?' }) ;
                var tokenStart = startIndex + 1;
                // Only find a single instance of a '}' after our start
                // character to avoid capturing escaped curly braces
                // in a regular expression constraint
                findEndBrace:
                    var endIndex = routeTemplate.IndexOf('}', tokenStart);
                    if (endIndex < routeTemplate.Length - 1 && routeTemplate[endIndex + 1] == '}')
                    {
                        tokenStart = endIndex + 2;
                        goto findEndBrace;
                    }

                routeTemplate = routeTemplate.Remove(startIndex, endIndex - startIndex);
            }
            
            return routeTemplate;
        }
    }
}
