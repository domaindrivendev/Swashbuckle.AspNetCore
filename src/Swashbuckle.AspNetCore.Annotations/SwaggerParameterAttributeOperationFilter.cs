using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using System.Reflection;

namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// An operation filter that adds parameter metadata on an action.
    /// defined by the <see cref="SwaggerParameterAttribute" /> on controller methods.
    /// </summary>
    /// <remarks>
    /// This filter overrides any existing description and sets the Required option only if set to true, otherwise it leaves it alone.
    /// </remarks>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter" />
    public class SwaggerParameterAttributeOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the specified operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="context">The context.</param>
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
            {
                return;
            }

            foreach (var parameter in operation.Parameters)
            {
                ApplyMetadata(parameter, context.ApiDescription);
            }
        }

        /// <summary>
        /// Applies action attribute metadata to the operation parameter.
        /// </summary>
        /// <param name="parameter">The operation parameter.</param>
        /// <param name="apiDescription">The API description</param>
        private static void ApplyMetadata(IParameter parameter, ApiDescription apiDescription)
        {
            // Find action parameter and attribute information matching the operation parameter.
            var attributeInfo = apiDescription
                .ActionDescriptor
                .Parameters
                .OfType<ControllerParameterDescriptor>()
                .Where(descriptor => parameter.Name.Equals((descriptor.BindingInfo?.BinderModelName ?? descriptor.Name), StringComparison.OrdinalIgnoreCase))
                .Select(descriptor => new { descriptor.Name, Attribute = GetSwaggerParameter(descriptor), Parameter = descriptor })
                .Where(attr => attr?.Attribute != null)
                .SingleOrDefault();

            if (attributeInfo == null)
            {
                return;
            }

            parameter.Description = attributeInfo.Attribute.Description;

            // Only override if required is set to true, meaning that if required is not set it will be whatever the implementation has detected it should be.
            // This allows for use of description without having to worry about the required parameter as well - though it prevents to option of using it to se a parameter as not required if the implementation thinks it is!
            parameter.Required = attributeInfo.Attribute.Required ? true : parameter.Required;
        }

        /// <summary>
        /// Gets the swagger parameter from the parameter descriptor, returns null if nothing is found
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns>A SwaggerParameterAttribute</returns>
        private static SwaggerParameterAttribute GetSwaggerParameter(ControllerParameterDescriptor descriptor)
        {
            if (descriptor.ParameterInfo == null)
            {
                return null;
            }

            return descriptor.ParameterInfo.GetCustomAttributes(false).OfType<SwaggerParameterAttribute>().SingleOrDefault();
        }
    }
}