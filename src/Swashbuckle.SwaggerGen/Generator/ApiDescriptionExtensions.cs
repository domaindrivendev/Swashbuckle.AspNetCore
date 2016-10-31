using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Swashbuckle.SwaggerGen.Generator
{
    public static class ApiDescriptionExtensions
    {
        internal static string RelativePathSansQueryString(this ApiDescription apiDescription)
        {
            return apiDescription.RelativePath.Split('?').First();
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

        internal static IEnumerable<string> SupportedRequestMediaTypes(this ApiDescription apiDescription)
        {
            return apiDescription.SupportedRequestFormats
                .Select(requestFormat => requestFormat.MediaType);
        }

        internal static IEnumerable<string> SupportedResponseMediaTypes(this ApiDescription apiDescription)
        {
            return apiDescription.SupportedResponseTypes
                .SelectMany(responseType => responseType.ApiResponseFormats)
                .Select(responseFormat => responseFormat.MediaType)
                .Distinct();
        }

        public static string ControllerName(this ApiDescription apiDescription)
        {
            var actionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
            return (actionDescriptor == null) ? null : actionDescriptor.ControllerName;
        }

        public static IEnumerable<object> ControllerAttributes(this ApiDescription apiDescription)
        {
            var actionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
            return (actionDescriptor == null)
                ? Enumerable.Empty<object>()
                : actionDescriptor.ControllerTypeInfo.GetCustomAttributes(false);
        }

        public static IEnumerable<object> ActionAttributes(this ApiDescription apiDescription)
        {
            var actionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
            return (actionDescriptor == null)
                ? Enumerable.Empty<object>()
                : actionDescriptor.MethodInfo.GetCustomAttributes(false);
        }

        internal static bool IsObsolete(this ApiDescription apiDescription)
        {
            return apiDescription.ActionAttributes().OfType<ObsoleteAttribute>().Any();
        }
    }
}