using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        private static readonly Type[] RequiredAttributeTypes = new[]
        {
            typeof(BindRequiredAttribute),
            typeof(RequiredAttribute)
        };

        public static bool IsRequiredParameter(this ApiParameterDescription apiParameter)
        {
            // From the OpenAPI spec:
            // If the parameter location is "path", this property is REQUIRED and its value MUST be true.
            if (apiParameter.IsFromPath())
            {
                return true;
            }

            // This is the default logic for IsRequired
            bool IsRequired() => apiParameter.CustomAttributes().Any(attr => RequiredAttributeTypes.Contains(attr.GetType()));

            // This is to keep compatibility with MVC controller logic that has existed in the past
            if (apiParameter.ParameterDescriptor is ControllerParameterDescriptor)
            {
                return IsRequired();
            }

            // For non-controllers, prefer the IsRequired flag if we're not on netstandard 2.0, otherwise fallback to the default logic.
            return
#if !NETSTANDARD2_0
            apiParameter.IsRequired;
#else
            IsRequired();
#endif
        }

        public static ParameterInfo ParameterInfo(this ApiParameterDescription apiParameter)
        {
            var parameterDescriptor = apiParameter.ParameterDescriptor as
#if NETCOREAPP2_2_OR_GREATER
                Microsoft.AspNetCore.Mvc.Infrastructure.IParameterInfoParameterDescriptor;
#else
                ControllerParameterDescriptor;
#endif

            return parameterDescriptor?.ParameterInfo;
        }

        public static PropertyInfo PropertyInfo(this ApiParameterDescription apiParameter)
        {
            var modelMetadata = apiParameter.ModelMetadata;

            return (modelMetadata?.ContainerType != null)
                ? modelMetadata.ContainerType.GetProperty(modelMetadata.PropertyName)
                : null;
        }

        public static IEnumerable<object> CustomAttributes(this ApiParameterDescription apiParameter)
        {
            var propertyInfo = apiParameter.PropertyInfo();
            if (propertyInfo != null) return propertyInfo.GetCustomAttributes(true);

            var parameterInfo = apiParameter.ParameterInfo();
            if (parameterInfo != null) return parameterInfo.GetCustomAttributes(true);

            return Enumerable.Empty<object>();
        }

        [Obsolete("Use ParameterInfo(), PropertyInfo() and CustomAttributes() extension methods instead")]
        internal static void GetAdditionalMetadata(
            this ApiParameterDescription apiParameter,
            ApiDescription apiDescription,
            out ParameterInfo parameterInfo,
            out PropertyInfo propertyInfo,
            out IEnumerable<object> parameterOrPropertyAttributes)
        {
            parameterInfo = apiParameter.ParameterInfo();
            propertyInfo = apiParameter.PropertyInfo();
            parameterOrPropertyAttributes = apiParameter.CustomAttributes();
        }

        internal static bool IsFromPath(this ApiParameterDescription apiParameter)
        {
            return (apiParameter.Source == BindingSource.Path);
        }

        internal static bool IsFromBody(this ApiParameterDescription apiParameter)
        {
            return (apiParameter.Source == BindingSource.Body);
        }

        internal static bool IsFromForm(this ApiParameterDescription apiParameter)
        {
            var source = apiParameter.Source;
            var elementType = apiParameter.ModelMetadata?.ElementType;

            return (source == BindingSource.Form || source == BindingSource.FormFile)
                || (elementType != null && typeof(IFormFile).IsAssignableFrom(elementType));
        }
    }
}