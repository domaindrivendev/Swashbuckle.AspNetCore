using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.Mvc.ApiExplorer;
using Microsoft.AspNet.Mvc.Controllers;

namespace Swashbuckle.Swagger
{
    public static class ApiDescriptionExtensions
    {
        public static IEnumerable<string> Produces(this ApiDescription apiDescription)
        {
            return apiDescription.SupportedResponseFormats
                .Select(format => format.MediaType.MediaType)
                .Distinct();
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
                ? actionDescriptor.ControllerTypeInfo.GetCustomAttributes(false)
                : Enumerable.Empty<object>();
        }

        public static IEnumerable<object> GetActionAttributes(this ApiDescription apiDescription)
        {
            var actionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
            return (actionDescriptor != null)
                ? actionDescriptor.MethodInfo.GetCustomAttributes(false)
                : Enumerable.Empty<object>();
        }
    }
}