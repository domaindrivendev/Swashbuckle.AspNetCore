using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations
{
    public class SwaggerParameterAttributeFilter : IParameterFilter
    {
        public void Apply(IParameter parameter, ParameterFilterContext context)
        {
            if (context.ParameterInfo == null) return;

            var customattributes = context.ParameterInfo.GetCustomAttributes(true);

            ApplySwaggerParameterAttribute(parameter, customattributes);
        }

        private void ApplySwaggerParameterAttribute(IParameter parameter, IEnumerable<object> customAttributes)
        {
            var swaggerParameterAttribute = customAttributes
                .OfType<SwaggerParameterAttribute>()
                .FirstOrDefault();

            if (swaggerParameterAttribute == null) return;

            parameter.Description = swaggerParameterAttribute.Description;
        }
    }
}