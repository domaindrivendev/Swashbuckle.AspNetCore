using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    /// <summary>
    /// Used to implement a logic of operation metadata modification before Swagger document generation.
    /// </summary>
    public interface IOperationFilter
    {
        /// <summary>
        /// Applies operation metadata modification logic to each operation.
        /// </summary>
        /// <param name="operation">Operation.</param>
        /// <param name="context">Operation context.</param>
        void Apply(Operation operation, OperationFilterContext context);
    }

    public class OperationFilterContext
    {
        public OperationFilterContext(ApiDescription apiDescription, ISchemaRegistry schemaRegistry)
        {
            ApiDescription = apiDescription;
            SchemaRegistry = schemaRegistry;
        }

        public ApiDescription ApiDescription { get; private set; }

        public ISchemaRegistry SchemaRegistry { get; private set; }
    }
}
