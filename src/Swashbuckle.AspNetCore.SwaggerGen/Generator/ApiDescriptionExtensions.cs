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
        public static MethodInfo MethodInfo(this ApiDescription apiDescription)
        {
            var controllerActionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
            return controllerActionDescriptor?.MethodInfo;
        }

        public static IEnumerable<object> CustomAttributes(this ApiDescription apiDescription)
        {
            var methodInfo = apiDescription.MethodInfo();

            if (methodInfo == null) return Enumerable.Empty<object>();

            return methodInfo.GetCustomAttributes(true)
                .Union(methodInfo.DeclaringType.GetCustomAttributes(true));
        }

        [Obsolete("Use MethodInfo() and CustomAttributes() extension methods instead")]
        public static void GetAdditionalMetadata(this ApiDescription apiDescription,
            out MethodInfo methodInfo,
            out IEnumerable<object> customAttributes)
        {
            methodInfo = apiDescription.MethodInfo();
            customAttributes = apiDescription.CustomAttributes();
        }

        internal static string RelativePathSansQueryString(this ApiDescription apiDescription)
        {
            return apiDescription.RelativePath.Split('?').First();
        }
    }
}
