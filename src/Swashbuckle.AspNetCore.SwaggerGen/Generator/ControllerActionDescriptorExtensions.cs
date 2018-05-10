using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class ControllerActionDescriptorExtensions
    {
        public static IEnumerable<object> GetControllerAndActionAttributes(
            this ControllerActionDescriptor controllerActionDescriptor,
            bool inherit)
        {
            var controllerAttributes = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes(inherit);
            var actionAttributes = controllerActionDescriptor.MethodInfo.GetCustomAttributes(inherit);

            return actionAttributes.Union(controllerAttributes);
        }

    }
}
