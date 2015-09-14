using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Swashbuckle.Swagger.Annotations
{
    public class ApplySwaggerResponseAttributes : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var apiDesc = context.ApiDescription;
            if (apiDesc.GetControllerAttributes().OfType<SwaggerResponseRemoveDefaultsAttribute>().Any() ||
                apiDesc.GetActionAttributes().OfType<SwaggerResponseRemoveDefaultsAttribute>().Any())
            {
                operation.Responses.Clear();
            }

            var controllerAttributes = apiDesc.GetControllerAttributes().OfType<SwaggerResponseAttribute>()
                .OrderBy(attr => attr.StatusCode);
            ApplyResponsesFrom(controllerAttributes, operation, context.SchemaDefinitions, context.SchemaProvider);

            var actionAttributes = apiDesc.GetActionAttributes().OfType<SwaggerResponseAttribute>()
                .OrderBy(attr => attr.StatusCode);
            ApplyResponsesFrom(actionAttributes, operation, context.SchemaDefinitions, context.SchemaProvider);
        }

        private void ApplyResponsesFrom(
            IOrderedEnumerable<SwaggerResponseAttribute> attributes,
            Operation operation,
            IDictionary<string, Schema> schemaDefinitions,
            ISchemaProvider schemaProvider)
        {
            foreach (var attr in attributes)
            {
                var statusCode = attr.StatusCode.ToString();

                operation.Responses[statusCode] = new Response
                {
                    Description = attr.Description ?? InferDescriptionFrom(statusCode),
                    Schema = (attr.Type == null)
                        ? null
                        : schemaProvider.GetSchema(attr.Type, schemaDefinitions)
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