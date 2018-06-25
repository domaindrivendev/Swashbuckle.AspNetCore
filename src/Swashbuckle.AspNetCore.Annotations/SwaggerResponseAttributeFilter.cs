using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations
{
    public class SwaggerResponseAttributeFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.MethodInfo == null) return;

            var swaggerResponseAttributes = context.MethodInfo.GetCustomAttributes(true)
                .Union(context.MethodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes(true))
                .OfType<SwaggerResponseAttribute>();

            if (!swaggerResponseAttributes.Any())
                return;

            if (operation.Responses == null)
            {
                operation.Responses = new Dictionary<string, Response>();
            }

            foreach (var attribute in swaggerResponseAttributes)
            {
                ApplyAttribute(operation, context, attribute);
            }
        }

        private static void ApplyAttribute(Operation operation, OperationFilterContext context, SwaggerResponseAttribute attribute)
        {
            var key = attribute.StatusCode.ToString();
            if (!operation.Responses.TryGetValue(key, out Response response))
            {
                response = new Response();
            }

            if (attribute.Description != null)
                response.Description = attribute.Description;

            if (attribute.Type != null && attribute.Type != typeof(void))
                response.Schema = context.SchemaRegistry.GetOrRegister(attribute.Type);

            operation.Responses[key] = response;
        }
    }
}
