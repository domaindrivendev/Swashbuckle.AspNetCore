using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class ApiParameterDescriptionExtensions
    {
        internal static bool TryGetParameterInfo(
            this ApiParameterDescription apiParameterDescription,
            ApiDescription apiDescription,
            out ParameterInfo parameterInfo)
        {
            var controllerParameterDescriptor = apiDescription.ActionDescriptor.Parameters
                .OfType<ControllerParameterDescriptor>()
                .FirstOrDefault(descriptor =>
                {
                    return (apiParameterDescription.Name == descriptor.BindingInfo?.BinderModelName)
                        || (apiParameterDescription.Name == descriptor.Name);
                });

            parameterInfo = controllerParameterDescriptor?.ParameterInfo;

            return (parameterInfo != null);
        }

        internal static bool TryGetPropertyInfo(
            this ApiParameterDescription apiParameterDescription,
            out PropertyInfo propertyInfo)
        {
            var modelMetadata = apiParameterDescription.ModelMetadata;

            propertyInfo = (modelMetadata?.ContainerType != null)
                ? modelMetadata.ContainerType.GetProperty(modelMetadata.PropertyName)
                : null;

            return (propertyInfo != null);
        }
    }
}