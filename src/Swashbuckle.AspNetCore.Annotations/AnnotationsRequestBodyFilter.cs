using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations;

public class AnnotationsRequestBodyFilter : IRequestBodyFilter
{
    public void Apply(IOpenApiRequestBody requestBody, RequestBodyFilterContext context)
    {
        var bodyParameterDescription = context.BodyParameterDescription;

        if (bodyParameterDescription == null)
        {
            return;
        }

        var propertyInfo = bodyParameterDescription.PropertyInfo();
        if (propertyInfo != null)
        {
            ApplyPropertyAnnotations(requestBody, propertyInfo);
            return;
        }

        var parameterInfo = bodyParameterDescription.ParameterInfo();
        if (parameterInfo != null)
        {
            ApplyParamAnnotations(requestBody, parameterInfo);
            return;
        }
    }

    private static void ApplyPropertyAnnotations(IOpenApiRequestBody parameter, PropertyInfo propertyInfo)
    {
        var swaggerRequestBodyAttribute = propertyInfo.GetCustomAttributes<SwaggerRequestBodyAttribute>()
            .FirstOrDefault();

        if (swaggerRequestBodyAttribute != null)
        {
            ApplySwaggerRequestBodyAttribute(parameter, swaggerRequestBodyAttribute);
        }
    }

    private static void ApplyParamAnnotations(IOpenApiRequestBody requestBody, ParameterInfo parameterInfo)
    {
        var swaggerRequestBodyAttribute = parameterInfo.GetCustomAttribute<SwaggerRequestBodyAttribute>();

        if (swaggerRequestBodyAttribute != null)
        {
            ApplySwaggerRequestBodyAttribute(requestBody, swaggerRequestBodyAttribute);
        }
    }

    private static void ApplySwaggerRequestBodyAttribute(IOpenApiRequestBody parameter, SwaggerRequestBodyAttribute swaggerRequestBodyAttribute)
    {
        if (swaggerRequestBodyAttribute.Description is { } description)
        {
            parameter.Description = description;
        }

        if (parameter is OpenApiRequestBody concrete &&
            swaggerRequestBodyAttribute.RequiredFlag is { } required)
        {
            concrete.Required = required;
        }
    }
}
