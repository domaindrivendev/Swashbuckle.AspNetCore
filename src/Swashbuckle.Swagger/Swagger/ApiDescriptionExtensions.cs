using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ApiExplorer;
using System;

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
            var actionDescriptor = apiDescription.ActionDescriptor;
            return (actionDescriptor.Properties.ContainsKey("ControllerAttributes"))
                ? (object[])actionDescriptor.Properties["ControllerAttributes"]
                : new object[] { };
        }

        public static IEnumerable<object> GetActionAttributes(this ApiDescription apiDescription)
        {
            var actionDescriptor = apiDescription.ActionDescriptor;
            return (actionDescriptor.Properties.ContainsKey("ActionAttributes"))
                ? (object[])actionDescriptor.Properties["ActionAttributes"]
                : new object[] { };
        }
    }
}