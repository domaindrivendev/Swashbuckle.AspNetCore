using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Basic.Swagger;

public class AssignOperationVendorExtensions : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
#if NET10_0_OR_GREATER
        operation.Extensions.Add("x-purpose", new OpenApiAny("test"));
#else
        operation.Extensions.Add("x-purpose", new OpenApiString("test"));
#endif
    }
}
