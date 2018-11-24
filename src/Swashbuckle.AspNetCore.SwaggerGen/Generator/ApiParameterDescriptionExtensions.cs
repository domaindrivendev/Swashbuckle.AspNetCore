using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class ApiParameterDescriptionExtensions
    {
        internal static void GetAdditionalMetadata(
            this ApiParameterDescription parameterDescription,
            ApiDescription apiDescription,
            out ParameterInfo parameterInfo,
            out PropertyInfo propertyInfo,
            out IEnumerable<object> parameterOrPropertyAttributes)
        {
            parameterInfo = null;
            propertyInfo = null;
            parameterOrPropertyAttributes = Enumerable.Empty<object>();

            if (parameterDescription.TryGetParameterInfo(apiDescription, out parameterInfo))
                parameterOrPropertyAttributes = parameterInfo.GetCustomAttributes(true);
            else if (parameterDescription.TryGetPropertyInfo(out propertyInfo))
                parameterOrPropertyAttributes = propertyInfo.GetCustomAttributes(true);
        }

        internal static bool IsFromPath(this ApiParameterDescription parameterDescription)
        {
            return (parameterDescription.Source == BindingSource.Path);
        }

        internal static bool IsFromBody(this ApiParameterDescription parameterDescription)
        {
            return (parameterDescription.Source == BindingSource.Body);
        }

        internal static bool IsFromForm(this ApiParameterDescription parameterDescription)
        {
            var source = parameterDescription.Source;
            var elementType = parameterDescription.ModelMetadata?.ElementType;

            return (source == BindingSource.Form || source == BindingSource.FormFile)
                || (elementType != null && typeof(IFormFile).IsAssignableFrom(elementType));
        }

        private static bool TryGetParameterInfo(
            this ApiParameterDescription parameterDescription,
            ApiDescription apiDescription,
            out ParameterInfo parameterInfo)
        {
            var controllerParameterDescriptor = apiDescription.ActionDescriptor.Parameters
                .OfType<ControllerParameterDescriptor>()
                .FirstOrDefault(descriptor =>
                {
                    return (parameterDescription.Name == descriptor.BindingInfo?.BinderModelName)
                        || (parameterDescription.Name == descriptor.Name);
                });

            parameterInfo = controllerParameterDescriptor?.ParameterInfo;

            return (parameterInfo != null);
        }

        private static bool TryGetPropertyInfo(
            this ApiParameterDescription parameterDescription,
            out PropertyInfo propertyInfo)
        {
            var modelMetadata = parameterDescription.ModelMetadata;

            propertyInfo = (modelMetadata?.ContainerType != null)
                ? modelMetadata.ContainerType.GetProperty(modelMetadata.PropertyName)
                : null;

            return (propertyInfo != null);
        }
    }
}