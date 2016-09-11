using Swashbuckle.SwaggerGen.Generator;
using Swashbuckle.Swagger.Model;

namespace Swashbuckle.SwaggerGen.Annotations
{
    internal class SwaggerResponseRemoveDefaultsOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            operation.Responses.Clear();
        }
    }
}
