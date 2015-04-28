using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.Mvc.Description;
using Microsoft.AspNet.Mvc;

namespace Swashbuckle.Swagger
{
    public static class ApiDescriptionExtensions
    {
        private const string ControllerAttributesKey = "Swashbuckle_ControllerAttributes";
        private const string ActionAttributesKey = "Swashbuckle_ActionAttributes";

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

        public static void SetActionAttributes(this ApiDescription apiDescription, IEnumerable<object> attributes)
        {
            apiDescription.Properties[ActionAttributesKey] = attributes;
        }

        public static IEnumerable<object> GetActionAttributes(this ApiDescription apiDescription)
        {
            return apiDescription.Properties.ContainsKey(ActionAttributesKey)
                ? (IEnumerable<object>)apiDescription.Properties[ActionAttributesKey]
                : InferActionAttributesFrom(apiDescription.ActionDescriptor);
        }
        public static void SetControllerAttributes(this ApiDescription apiDescription, IEnumerable<object> attributes)
        {
            apiDescription.Properties[ControllerAttributesKey] = attributes;
        }

        public static IEnumerable<object> GetControllerAttributes(this ApiDescription apiDescription)
        {
            return apiDescription.Properties.ContainsKey(ControllerAttributesKey)
                ? (IEnumerable<object>)apiDescription.Properties[ControllerAttributesKey]
                : InferControllerAttributesFrom(apiDescription.ActionDescriptor);
        }

        private static IEnumerable<object> InferActionAttributesFrom(ActionDescriptor actionDescriptor)
        {
            var controllerActionDescriptor = actionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor == null) return new Attribute[] { };

            return controllerActionDescriptor.MethodInfo.GetCustomAttributes(true);
        }

        private static IEnumerable<object> InferControllerAttributesFrom(ActionDescriptor actionDescriptor)
        {
            var controllerActionDescriptor = actionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor == null) return new Attribute[] { };

            return controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes(true);
        }
    }
}