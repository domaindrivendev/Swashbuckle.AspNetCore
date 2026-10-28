using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Net.Http.Headers;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public static class ApiParameterDescriptionExtensions
{
    private static readonly Type[] RequiredAttributeTypes =
    [
        typeof(BindRequiredAttribute),
        typeof(RequiredAttribute),
        typeof(System.Runtime.CompilerServices.RequiredMemberAttribute)
    ];

    private static readonly HashSet<string> IllegalHeaderParameters = new(StringComparer.OrdinalIgnoreCase)
    {
        HeaderNames.Accept,
        HeaderNames.Authorization,
        HeaderNames.ContentType
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
        bool IsRequired() => apiParameter.CustomAttributes().Any(IsRequiredAttribute);

        // This is to keep compatibility with MVC controller logic that has existed in the past
        if (apiParameter.ParameterDescriptor is ControllerParameterDescriptor)
        {
            return IsRequired() && !apiParameter.IsNestedInOptionalProperty();
        }

        // For non-controllers, prefer the IsRequired flag if we're not on netstandard 2.0, otherwise fallback to the default logic.
        return apiParameter.IsRequired;
    }

    private static bool IsRequiredAttribute(object attribute) => RequiredAttributeTypes.Contains(attribute.GetType());

    // A property of a nested complex type is flattened into a dotted name such as "Inner.Prop".
    // It is only required if every property on the path to it is required too. See #3534.
    private static bool IsNestedInOptionalProperty(this ApiParameterDescription apiParameter)
    {
        var segments = apiParameter.Name?.Split('.');
        var type = apiParameter.ParameterDescriptor?.ParameterType;

        if (segments is not { Length: > 1 } || type is null)
        {
            return false;
        }

        for (var i = 0; i < segments.Length - 1; i++)
        {
            var property = FindBoundProperty(type, segments[i]);
            if (property is null)
            {
                return false;
            }

            if (!property.GetCustomAttributes(true).Any(IsRequiredAttribute))
            {
                return true;
            }

            type = property.PropertyType;
        }

        return false;

        static PropertyInfo FindBoundProperty(Type type, string name) =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault((property) => string.Equals(
                    property.GetCustomAttributes(true).OfType<IModelNameProvider>().FirstOrDefault()?.Name ?? property.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase));
    }

    public static ParameterInfo ParameterInfo(this ApiParameterDescription apiParameter)
    {
        var parameterDescriptor = apiParameter.ParameterDescriptor as Microsoft.AspNetCore.Mvc.Infrastructure.IParameterInfoParameterDescriptor;
        return parameterDescriptor?.ParameterInfo;
    }

    public static PropertyInfo PropertyInfo(this ApiParameterDescription apiParameter)
    {
        var modelMetadata = apiParameter.ModelMetadata;

        return modelMetadata?.ContainerType?.GetProperty(modelMetadata.PropertyName, modelMetadata.ModelType);
    }

    public static IEnumerable<object> CustomAttributes(this ApiParameterDescription apiParameter)
    {
        var propertyInfo = apiParameter.PropertyInfo();
        if (propertyInfo != null)
        {
            return propertyInfo.GetCustomAttributes(true);
        }

        var parameterInfo = apiParameter.ParameterInfo();
        if (parameterInfo != null)
        {
            return parameterInfo.GetCustomAttributes(true);
        }

        return [];
    }

    internal static bool IsFromPath(this ApiParameterDescription apiParameter)
    {
        return apiParameter.Source == BindingSource.Path;
    }

    internal static bool IsFromBody(this ApiParameterDescription apiParameter)
    {
        return apiParameter.Source == BindingSource.Body;
    }

    internal static bool IsFromForm(this ApiParameterDescription apiParameter)
    {
        bool isEnhancedModelMetadataSupported = true;

#if NET9_0_OR_GREATER
        if (AppContext.TryGetSwitch("Microsoft.AspNetCore.Mvc.ApiExplorer.IsEnhancedModelMetadataSupported", out var isEnabled))
        {
            isEnhancedModelMetadataSupported = isEnabled;
        }
#endif

        var source = apiParameter.Source;
        var elementType = isEnhancedModelMetadataSupported ? apiParameter.ModelMetadata?.ElementType : null;

        return
            source == BindingSource.Form ||
            source == BindingSource.FormFile ||
            (elementType != null && typeof(IFormFile).IsAssignableFrom(elementType));
    }

    internal static bool IsIllegalHeaderParameter(this ApiParameterDescription apiParameter)
    {
        // Certain header parameters are not allowed and should be described using the corresponding OpenAPI keywords
        // https://swagger.io/docs/specification/describing-parameters/#header-parameters
        return apiParameter.Source == BindingSource.Header && IllegalHeaderParameters.Contains(apiParameter.Name);
    }
}
