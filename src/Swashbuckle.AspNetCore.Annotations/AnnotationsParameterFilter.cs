using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations
{
    public class AnnotationsParameterFilter : IParameterFilter
    {
        public void Apply(IParameter parameter, ParameterFilterContext context)
        {
            if (context.ParameterInfo == null) return;

            var customAttributes = context.ParameterInfo.GetCustomAttributes(true);

            ApplySwaggerParameterAttribute(parameter, customAttributes);
        }

        private void ApplySwaggerParameterAttribute(IParameter parameter, IEnumerable<object> customAttributes)
        {
            var swaggerParameterAttribute = customAttributes
                .OfType<SwaggerParameterAttribute>()
                .FirstOrDefault();

            if (swaggerParameterAttribute == null) return;

            if (swaggerParameterAttribute.Description != null)
                parameter.Description = swaggerParameterAttribute.Description;

            if (swaggerParameterAttribute.RequiredProvided)
                parameter.Required = swaggerParameterAttribute.Required;
        }
    }
}