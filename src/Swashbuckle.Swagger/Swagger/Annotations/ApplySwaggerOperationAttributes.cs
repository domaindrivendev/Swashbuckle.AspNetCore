using System.Linq;

namespace Swashbuckle.Swagger.Annotations
{
    public class ApplySwaggerOperationAttributes : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var attribute = context.ApiDescription.GetActionAttributes()
                .OfType<SwaggerOperationAttribute>()
                .FirstOrDefault();
            if (attribute == null) return;

            if (attribute.OperationId != null)
                operation.operationId = attribute.OperationId;
            
            if (attribute.Tags != null)
                operation.tags = attribute.Tags;

            if (attribute.Schemes != null)
                operation.schemes = attribute.Schemes;
        }
    }
}