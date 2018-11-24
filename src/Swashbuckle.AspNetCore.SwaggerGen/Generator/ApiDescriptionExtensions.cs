using System;
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
            var controllerActionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
            methodInfo = controllerActionDescriptor?.MethodInfo;

            return (methodInfo != null);
        }

        public static void GetAdditionalMetadata(
            this ApiDescription apiDescription,
            out MethodInfo methodInfo,
            out IEnumerable<object> methodAttributes)
        {
            methodAttributes = Enumerable.Empty<object>();

            if (apiDescription.TryGetMethodInfo(out methodInfo))
            {
                methodAttributes = methodInfo.GetCustomAttributes(true)
                    .Union(methodInfo.DeclaringType.GetCustomAttributes(true));
            }
        }

        internal static string RelativePathSansQueryString(this ApiDescription apiDescription)
        {
            return apiDescription.RelativePath.Split('?').First();
        }

        internal static bool IsObsolete(this ApiDescription apiDescription)
        {
            apiDescription.GetAdditionalMetadata(out MethodInfo methodInfo, out IEnumerable<object> methodAttributes);

            return methodAttributes.OfType<ObsoleteAttribute>().Any();
        }
    }
}
