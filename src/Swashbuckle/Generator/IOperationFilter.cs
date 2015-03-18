using Microsoft.AspNet.Mvc.Description;

namespace Swashbuckle.Swagger
{
    public interface IOperationFilter
    {
        void Apply(Operation operation, SchemaGenerator schemaRegistry, ApiDescription apiDescription);
    }
}
