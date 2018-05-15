using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface IOperationFilter
    {
        void Apply(Operation operation, OperationFilterContext context);
    }

    public class OperationFilterContext
    {
        public OperationFilterContext(
            ApiDescription apiDescription,
            ISchemaRegistry schemaRegistry)
        {
            ApiDescription = apiDescription;
            ControllerActionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
            SchemaRegistry = schemaRegistry;
        }

        public ApiDescription ApiDescription { get; private set; }

        public ControllerActionDescriptor ControllerActionDescriptor { get; }

        public ISchemaRegistry SchemaRegistry { get; private set; }

    }
}
