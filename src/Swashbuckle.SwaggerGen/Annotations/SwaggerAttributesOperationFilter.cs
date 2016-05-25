using System;
using System.Linq;
using System.Net;
using Swashbuckle.SwaggerGen.Generator;
using Swashbuckle.SwaggerGen.Extensions;

namespace Swashbuckle.SwaggerGen.Annotations
{
    public class SwaggerAttributesOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            ApplyOperationAttributes(operation, context);
            ApplyOperationFilterAttributes(operation, context);
            ApplyResponseRemoveDefaultsAttributes(operation, context);
            ApplyResponseAttributes(operation, context);
        }

        private static void ApplyOperationAttributes(Operation operation, OperationFilterContext context)
        {
            var attribute = context.ApiDescription.GetActionAttributes()
                .OfType<SwaggerOperationAttribute>()
                .FirstOrDefault();
            if (attribute == null) return;

            if (attribute.OperationId != null)
                operation.OperationId = attribute.OperationId;

            if (attribute.Tags != null)
                operation.Tags = attribute.Tags;

            if (attribute.Schemes != null)
                operation.Schemes = attribute.Schemes;
        }

        public static void ApplyOperationFilterAttributes(Operation operation, OperationFilterContext context)
        {
            var apiDesc = context.ApiDescription;

            var controllerAttributes = apiDesc.GetControllerAttributes().OfType<SwaggerOperationFilterAttribute>();
            var actionAttributes = apiDesc.GetActionAttributes().OfType<SwaggerOperationFilterAttribute>();

            foreach (var attribute in controllerAttributes.Union(actionAttributes))
            {
                var filter = (IOperationFilter)Activator.CreateInstance(attribute.FilterType);
                filter.Apply(operation, context);
            }
        }

        private static void ApplyResponseRemoveDefaultsAttributes(Operation operation, OperationFilterContext context)
        {
            var apiDesc = context.ApiDescription;
            if (apiDesc.GetControllerAttributes().OfType<SwaggerResponseRemoveDefaultsAttribute>().Any() ||
                apiDesc.GetActionAttributes().OfType<SwaggerResponseRemoveDefaultsAttribute>().Any())
            {
                operation.Responses.Clear();
            }
        }

        private static void ApplyResponseAttributes(Operation operation, OperationFilterContext context)
        {
            var apiDesc = context.ApiDescription;
            var attributes =
                apiDesc.GetControllerAttributes().OfType<SwaggerResponseAttribute>()
                .Union(apiDesc.GetActionAttributes().OfType<SwaggerResponseAttribute>())
                .OrderBy(attr => attr.StatusCode);

            foreach (var attribute in attributes)
                ApplyResponseAttribute(operation, context, attribute);
        }

        private static void ApplyResponseAttribute(
            Operation operation,
            OperationFilterContext context,
            SwaggerResponseAttribute attribute)
        {
            var statusCode = attribute.StatusCode.ToString();

            operation.Responses[statusCode] = new Response
            {
                Description = attribute.Description ?? InferDescriptionFrom(statusCode),
                Schema = (attribute.Type == null)
                    ? null
                    : context.SchemaRegistry.GetOrRegister(attribute.Type)
            };
        }

        private static string InferDescriptionFrom(string statusCode)
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