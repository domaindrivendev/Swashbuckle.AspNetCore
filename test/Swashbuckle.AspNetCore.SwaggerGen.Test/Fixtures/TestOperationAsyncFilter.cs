using System.Threading.Tasks;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class TestOperationAsyncFilter : IOperationAsyncFilter
    {
        public Task Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Extensions.Add("X-foo", new OpenApiString("bar"));
            operation.Extensions.Add("X-docName", new OpenApiString(context.DocumentName));

            return Task.CompletedTask;
        }
    }
}
