using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Description;

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
            return apiDescription.GetActionPropertyOrDefault("IsObsolete", false);
        }

        public static IEnumerable<object> GetControllerAttributes(this ApiDescription apiDescription)
        {
            return apiDescription.GetActionPropertyOrDefault("ControllerAttributes", new object[] { });
        }

        public static IEnumerable<object> GetActionAttributes(this ApiDescription apiDescription)
        {
            return apiDescription.GetActionPropertyOrDefault("ActionAttributes", new object[] { });
        }

        public static T GetActionPropertyOrDefault<T>(this ApiDescription apiDescription, string key, T defaultValue)
        {
            if (apiDescription.ActionDescriptor.Properties.ContainsKey(key))
            {
                var value = apiDescription.ActionDescriptor.Properties[key];
                return (T)value;
            }
            return defaultValue;
        }
    }
}