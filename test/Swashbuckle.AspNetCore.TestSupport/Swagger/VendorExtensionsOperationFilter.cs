using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.TestSupport
{
    public class VendorExtensionsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext contex)
        {
            operation.Extensions.Add("X-foo", new OpenApiString("bar"));
        }
    }
}