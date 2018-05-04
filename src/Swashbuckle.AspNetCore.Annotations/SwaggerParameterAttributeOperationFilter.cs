using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
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
        public void Apply(Operation operation, OperationFilterContext context)
        {
            EnsureParameters(operation);

            foreach (var pair in this.GetParameterAttributes(context))
            {
                var param = GetOrAddParameter(operation, pair.Key);
                param.Description = pair.Value.Description;

                // Only override if required is set to true, meaning that if required is not set it will be whatever the implementation has detected it should be.
                // This allows for use of description without having to worry about the required parameter as well - though it prevents to option of using it to se a parameter as not required if the implementation thinks it is!
                param.Required = pair.Value.Required ? true : param.Required;
            }
        }

        /// <summary>
        /// Ensures the parameters exists on the operation
        /// </summary>
        /// <param name="operation">The operation.</param>
        private static void EnsureParameters(Operation operation)
        {
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<IParameter>();
            }
        }

        /// <summary>
        /// Gets the or add parameter to the operation collection of parameters
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="name">The name of the parameter</param>
        /// <returns></returns>
        private static IParameter GetOrAddParameter(Operation operation, string name)
        {
            var parameter = operation.Parameters?.SingleOrDefault(p => p.Name == name);
            if (parameter == null)
            {
                parameter = new NonBodyParameter();
                operation.Parameters.Add(parameter);
            }

            return parameter;
        }

        /// <summary>
        /// Gets the parameter attributes from the operation
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A dictionary of parameter, attribute pairs</returns>
        private Dictionary<string, SwaggerParameterAttribute> GetParameterAttributes(OperationFilterContext context)
        {
            var controllerActionDescriptor = context.ApiDescription.ControllerActionDescriptor();

            if (controllerActionDescriptor?.Parameters == null)
            {
                return new Dictionary<string, SwaggerParameterAttribute>();
            }

            return controllerActionDescriptor.Parameters.OfType<ControllerParameterDescriptor>()
                .Select(param => new { Name = param.Name, Attribute = GetSwaggerParameter(param) })
                .Where(attr => attr?.Attribute != null)
                .ToDictionary(k => k.Name, v => v.Attribute);
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