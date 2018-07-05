using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class ApiDescriptionExtensions
    {
        [Obsolete("Deprecated: Use TryGetMethodInfo")]
        public static IEnumerable<object> ControllerAttributes(this ApiDescription apiDescription)
        {
            var controllerActionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
            return (controllerActionDescriptor == null)
                ? Enumerable.Empty<object>()
                : controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes(true);
        }

        [Obsolete("Deprecated: Use TryGetMethodInfo")]
        public static IEnumerable<object> ActionAttributes(this ApiDescription apiDescription)
        {
            var controllerActionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
            return (controllerActionDescriptor == null)
                ? Enumerable.Empty<object>()
                : controllerActionDescriptor.MethodInfo.GetCustomAttributes(true);
        }

        public static bool TryGetMethodInfo(this ApiDescription apiDescription, out MethodInfo methodInfo)
        {
            var controllerActionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;

            methodInfo = controllerActionDescriptor?.MethodInfo;

            return (methodInfo != null);
        }

        internal static string FriendlyId(this ApiDescription apiDescription)
        {
            var parts = (apiDescription.RelativePathSansQueryString() + "/" + apiDescription.HttpMethod.ToLower())
                .Split('/');

            var builder = new StringBuilder();
            foreach (var part in parts) 
            {
                var trimmed = part.Trim('{', '}');
                builder.AppendFormat("{0}{1}",
                    (part.StartsWith("{") ? "By" : string.Empty),
                    trimmed.ToTitleCase()
                );
            }

            return builder.ToString();
        }

        internal static string RelativePathSansQueryString(this ApiDescription apiDescription)
        {
            return apiDescription.RelativePath.Split('?').First();
        }

        internal static IEnumerable<string> SupportedRequestMediaTypes(this ApiDescription apiDescription)
        {
            return apiDescription.SupportedRequestFormats
                .Select(requestFormat => requestFormat.MediaType)
                .Distinct();
        }

        internal static IEnumerable<string> SupportedResponseMediaTypes(this ApiDescription apiDescription)
        {
            return apiDescription.SupportedResponseTypes
                .SelectMany(responseType => responseType.ApiResponseFormats)
                .Select(responseFormat => responseFormat.MediaType)
                .Distinct();
        }

        internal static bool IsObsolete(this ApiDescription apiDescription)
        {
            if (!apiDescription.TryGetMethodInfo(out MethodInfo methodInfo))
                return false;

            return methodInfo.GetCustomAttributes(true)
                .Union(methodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes(true))
                .Any(attr => attr.GetType() == typeof(ObsoleteAttribute));
        }
    }
}
