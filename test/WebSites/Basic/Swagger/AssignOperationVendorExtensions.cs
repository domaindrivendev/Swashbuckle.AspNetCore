using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Basic.Swagger;

public class AssignOperationVendorExtensions : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        operation.Extensions.Add("x-purpose", new JsonNodeExtension("test"));
    }
}
