using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Description;
using Microsoft.AspNet.Mvc;

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
            return apiDescription.AttributesOfType<ObsoleteAttribute>().Any();
        }

        public static IEnumerable<TAttribute> AttributesOfType<TAttribute>(this ApiDescription apiDescription)
        {
            var controllerActionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor == null) return new TAttribute[] { };

            return controllerActionDescriptor.MethodInfo.GetCustomAttributes(true).OfType<TAttribute>();
        }
    }
}