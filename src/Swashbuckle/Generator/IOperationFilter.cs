using Microsoft.AspNet.Mvc.Description;

namespace Swashbuckle.Swagger.Generator
{
    public interface IOperationFilter
    {
        void Apply(Operation operation, SchemaGenerator schemaRegistry, ApiDescription apiDescription);
    }
}
