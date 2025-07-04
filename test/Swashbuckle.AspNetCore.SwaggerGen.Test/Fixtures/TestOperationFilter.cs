using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class TestOperationFilter : IOperationFilter, IOperationAsyncFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        operation.Extensions.Add("X-foo", new JsonNodeExtension("bar"));
        operation.Extensions.Add("X-docName", new JsonNodeExtension(context.DocumentName));
    }

    public Task ApplyAsync(OpenApiOperation operation, OperationFilterContext context, CancellationToken cancellationToken)
    {
        Apply(operation, context);
        return Task.CompletedTask;
    }
}
