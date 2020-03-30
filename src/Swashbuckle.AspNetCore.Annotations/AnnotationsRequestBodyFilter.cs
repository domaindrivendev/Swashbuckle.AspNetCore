using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations
{
    public class AnnotationsRequestBodyFilter : IRequestBodyFilter
    {
        public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
        {
            var bodyParameterDescription = context.BodyParameterDescription;

            if (bodyParameterDescription == null) return;

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

        private void ApplyPropertyAnnotations(OpenApiRequestBody parameter, PropertyInfo propertyInfo)
        {
            var swaggerRequestBodyAttribute = propertyInfo.GetCustomAttributes<SwaggerRequestBodyAttribute>()
                .FirstOrDefault();

            if (swaggerRequestBodyAttribute != null)
                ApplySwaggerRequestBodyAttribute(parameter, swaggerRequestBodyAttribute);
        }

        private void ApplyParamAnnotations(OpenApiRequestBody requestBody, ParameterInfo parameterInfo)
        {
            var swaggerRequestBodyAttribute = parameterInfo.GetCustomAttributes<SwaggerRequestBodyAttribute>()
                .FirstOrDefault();

            if (swaggerRequestBodyAttribute != null)
                ApplySwaggerRequestBodyAttribute(requestBody, swaggerRequestBodyAttribute);
        }

        private void ApplySwaggerRequestBodyAttribute(OpenApiRequestBody parameter, SwaggerRequestBodyAttribute swaggerRequestBodyAttribute)
        {
            if (swaggerRequestBodyAttribute.Description != null)
                parameter.Description = swaggerRequestBodyAttribute.Description;

            if (swaggerRequestBodyAttribute.RequiredFlag.HasValue)
                parameter.Required = swaggerRequestBodyAttribute.RequiredFlag.Value;
        }
    }
}