using System;
using System.Linq;
using System.Net;

namespace Swashbuckle.Swagger.Annotations
{
    public class AddResponsesFromAttributes : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var apiDesc = context.ApiDescription;
            if (apiDesc.GetControllerAttributes().OfType<SwaggerResponseRemoveDefaultsAttribute>().Any() ||
                apiDesc.GetActionAttributes().OfType<SwaggerResponseRemoveDefaultsAttribute>().Any())
            {
                operation.responses.Clear();
            }

            var controllerAttributes = apiDesc.GetControllerAttributes().OfType<SwaggerResponseAttribute>()
                .OrderBy(attr => attr.StatusCode);
            ApplyResponsesFrom(operation, controllerAttributes, context.SchemaRegistry);

            var actionAttributes = apiDesc.GetActionAttributes().OfType<SwaggerResponseAttribute>()
                .OrderBy(attr => attr.StatusCode);
            ApplyResponsesFrom(operation, actionAttributes, context.SchemaRegistry);
        }

        private void ApplyResponsesFrom(
            Operation operation,
            IOrderedEnumerable<SwaggerResponseAttribute> attributes,
            ISchemaRegistry schemaRegistry)
        {
            foreach (var attr in attributes)
            {
                var statusCode = attr.StatusCode.ToString();

                operation.responses[statusCode] = new Response
                {
                    description = attr.Description ?? InferDescriptionFrom(statusCode),
                    schema = (attr.Type != null) ? schemaRegistry.GetOrRegister(attr.Type) : null
                };
            }
        }

        private string InferDescriptionFrom(string statusCode)
        {
            HttpStatusCode enumValue;
            if (Enum.TryParse(statusCode, true, out enumValue))
            {
                return enumValue.ToString();
            }
            return null;
        }
    }
}