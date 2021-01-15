using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class VendorExtensionsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Extensions.Add("X-foo", new OpenApiString("bar"));
            operation.Extensions.Add("X-docName", new OpenApiString(context.DocumentName));
        }
    }
}