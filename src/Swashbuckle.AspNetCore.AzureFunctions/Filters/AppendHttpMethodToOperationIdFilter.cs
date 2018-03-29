using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.AzureFunctions.Filters
{
    /// <summary>
    /// Operation filter, that adds http method to OperationId. One azure function can handle multiple http methods
    /// </summary>
    public class AppendHttpMethodToOperationIdFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            operation.OperationId += $"_{context.ApiDescription.HttpMethod}";
        }
    }
}