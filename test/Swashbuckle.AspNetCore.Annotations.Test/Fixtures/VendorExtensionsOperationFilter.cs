using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class VendorExtensionsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
#if NET10_0_OR_GREATER
            operation.Extensions.Add("X-property1", new OpenApiAny("value"));
#else
            operation.Extensions.Add("X-property1", new OpenApiString("value"));
#endif
        }
    }
}
