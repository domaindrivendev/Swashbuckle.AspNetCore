using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations.Test;

public class VendorExtensionsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        operation.Extensions.Add("X-property1", new JsonNodeExtension("value"));
    }
}
