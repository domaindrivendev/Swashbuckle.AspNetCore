using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;

namespace Swashbuckle.SwaggerGen.Annotations
{
    /// <summary>
    /// Operation filter that adds Swagger response information to an operation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This filter applies the values specified in <see cref="SwaggerProducesResponseTypeAttribute"/>
    /// settings to an operation. This will override or append to the set of responses
    /// as needed.
    /// </para>
    /// </remarks>
    /// <seealso cref="SwaggerProducesResponseTypeAttribute" />
    public class SwaggerProducesResponseTypeFilter : IOperationFilter
    {
        /// <summary>
        /// Applies the filter specified operation.
        /// </summary>
        /// <param name="operation">The operation to which the filter is being applied.</param>
        /// <param name="context">The filter context containing information to help in applying the filter.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="operation" /> or <paramref name="context" /> is <see langword="null" />.
        /// </exception>
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var apiDesc = context.ApiDescription;
            var attributes = GetActionAttributes(apiDesc);

            if (!attributes.Any())
            {
                return;
            }

            if (operation.Responses == null)
            {
                operation.Responses = new Dictionary<string, Response>();
            }

            foreach (var attribute in attributes)
            {
                ApplyAttribute(operation, context, attribute);
            }
        }

        private static void ApplyAttribute(Operation operation, OperationFilterContext context, SwaggerProducesResponseTypeAttribute attribute)
        {
            var key = attribute.StatusCode.ToString();
            Response response;
            if (!operation.Responses.TryGetValue(key, out response))
            {
                response = new Response();
            }

            response.Description = attribute.Description;
            if (attribute.Type != null && attribute.Type != typeof(void))
            {
                response.Schema = context.SchemaRegistry.GetOrRegister(attribute.Type);
            }

            operation.Responses[key] = response;
        }

        private static IEnumerable<SwaggerProducesResponseTypeAttribute> GetActionAttributes(ApiDescription apiDesc)
        {
            var controllerAttributes = apiDesc.ControllerAttributes().OfType<SwaggerProducesResponseTypeAttribute>();
            var actionAttributes = apiDesc.ActionAttributes().OfType<SwaggerProducesResponseTypeAttribute>();
            return controllerAttributes.Union(actionAttributes);
        }
    }
}
