using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// An operation filter that applies tags based on any <see cref="SwaggerTagAttribute" /> instances on the controller
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter" />
    public class SwaggerTagAttributeOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var controllerTags = GetControllerTagNames(operation, context).ToList();

            if (operation.Tags != null)
            {
                operation.Tags = controllerTags.Union(operation.Tags).ToList();
            }
            else
            {
                operation.Tags = controllerTags;
            }
        }

        /// <summary>
        /// Gets the controller tag names as defined by any <see cref="SwaggerTagAttribute" /> instances.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="context">The context.</param>
        /// <returns>A collection of tags</returns>
        private static IEnumerable<string> GetControllerTagNames(Operation operation, OperationFilterContext context)
        {
            return context.ApiDescription
                           .ControllerAttributes()
                           .OfType<SwaggerTagAttribute>()
                           .Where(tag => !string.IsNullOrWhiteSpace(tag.Name))
                           .Select(tag => tag.Name)
                           .Where(tag => !operation.Tags.Contains(tag));
        }
    }
}
