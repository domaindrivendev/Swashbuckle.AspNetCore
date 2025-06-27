using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations;

public class AnnotationsParameterFilter : IParameterFilter
{
    public void Apply(IOpenApiParameter parameter, ParameterFilterContext context)
    {
        if (context.PropertyInfo is { } propertyInfo)
        {
            ApplyPropertyAnnotations(parameter, propertyInfo);
        }

        if (context.ParameterInfo is { } parameterInfo)
        {
            ApplyParamAnnotations(parameter, parameterInfo);
        }
    }

    private static void ApplyPropertyAnnotations(IOpenApiParameter parameter, PropertyInfo propertyInfo)
    {
        var swaggerParameterAttribute = propertyInfo.GetCustomAttributes<SwaggerParameterAttribute>()
            .FirstOrDefault();

        if (swaggerParameterAttribute != null)
        {
            ApplySwaggerParameterAttribute(parameter, swaggerParameterAttribute);
        }
    }

    private static void ApplyParamAnnotations(IOpenApiParameter parameter, ParameterInfo parameterInfo)
    {
        var swaggerParameterAttribute = parameterInfo.GetCustomAttribute<SwaggerParameterAttribute>();

        if (swaggerParameterAttribute != null)
        {
            ApplySwaggerParameterAttribute(parameter, swaggerParameterAttribute);
        }
    }

    private static void ApplySwaggerParameterAttribute(IOpenApiParameter parameter, SwaggerParameterAttribute swaggerParameterAttribute)
    {
        if (swaggerParameterAttribute.Description is { } description)
        {
            parameter.Description = description;
        }

        if (parameter is OpenApiParameter concrete &&
            swaggerParameterAttribute.RequiredFlag is { } required)
        {
            concrete.Required = required;
        }
    }
}
