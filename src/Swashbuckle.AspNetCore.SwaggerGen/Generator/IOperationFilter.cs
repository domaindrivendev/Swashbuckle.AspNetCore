using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface IOperationFilter
    {
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
