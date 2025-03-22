using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations;

public class AnnotationsParameterFilter : IParameterFilter
{
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (context.PropertyInfo != null)
        {
            ApplyPropertyAnnotations(parameter, context.PropertyInfo);
        }
        else if (context.ParameterInfo != null)
        {
            ApplyParamAnnotations(parameter, context.ParameterInfo);
        }
    }

    private static void ApplyPropertyAnnotations(OpenApiParameter parameter, PropertyInfo propertyInfo)
    {
        var swaggerParameterAttribute = propertyInfo.GetCustomAttributes<SwaggerParameterAttribute>()
            .FirstOrDefault();

        if (swaggerParameterAttribute != null)
        {
            ApplySwaggerParameterAttribute(parameter, swaggerParameterAttribute);
        }
    }

    private static void ApplyParamAnnotations(OpenApiParameter parameter, ParameterInfo parameterInfo)
    {
        var swaggerParameterAttribute = parameterInfo.GetCustomAttribute<SwaggerParameterAttribute>();

        if (swaggerParameterAttribute != null)
        {
            ApplySwaggerParameterAttribute(parameter, swaggerParameterAttribute);
        }
    }

    private static void ApplySwaggerParameterAttribute(OpenApiParameter parameter, SwaggerParameterAttribute swaggerParameterAttribute)
    {
        if (swaggerParameterAttribute.Description != null)
        {
            parameter.Description = swaggerParameterAttribute.Description;
        }

        if (swaggerParameterAttribute.RequiredFlag.HasValue)
        {
            parameter.Required = swaggerParameterAttribute.RequiredFlag.Value;
        }
    }
}
