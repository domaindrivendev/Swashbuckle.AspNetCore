using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Swashbuckle.SwaggerGen.Generator
{
    public static class ApiDescriptionExtensions
    {
        public static string FriendlyId(this ApiDescription apiDescription)
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

        public static IEnumerable<string> Produces(this ApiDescription apiDescription)
        {
            List<string> formates = new List<string>();

            foreach (var respTypes in apiDescription.SupportedResponseTypes)
            {
                if (respTypes.ApiResponseFormats != null)
                {
                    foreach (var respFormats in respTypes.ApiResponseFormats)
                    {
                        formates.Add(respFormats.MediaType);
                    }
                }
            }

            return formates.Distinct();

            //return apiDescription.SupportedResponseFormats
            //    .Select(format => format.MediaType)
            //    .Distinct();
        }

        public static Type FirstOrDefaultResponseType(this ApiDescription apiDescription)
        {
            if (apiDescription?.SupportedResponseTypes == null || apiDescription.SupportedResponseTypes.Count <= 0)
            {
                return typeof(void);
            }

            return apiDescription.SupportedResponseTypes[0].Type;
        }

        public static string RelativePathSansQueryString(this ApiDescription apiDescription)
        {
            return apiDescription.RelativePath.Split('?').First();
        }

        public static bool IsObsolete(this ApiDescription apiDescription)
        {
            return apiDescription.GetActionAttributes().OfType<ObsoleteAttribute>().Any();
        }

        public static IEnumerable<object> GetControllerAttributes(this ApiDescription apiDescription)
        {
            var actionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
            return (actionDescriptor != null)
                ? actionDescriptor.ControllerTypeInfo.GetType().GetCustomAttributes(false)
                : Enumerable.Empty<object>();
        }

        public static IEnumerable<object> GetActionAttributes(this ApiDescription apiDescription)
        {
            var actionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
            return (actionDescriptor != null)
                ? actionDescriptor.MethodInfo.GetType().GetCustomAttributes(false)
                : Enumerable.Empty<object>();
        }
    }
}