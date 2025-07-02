using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class TestOperationFilter : IOperationFilter, IOperationAsyncFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Extensions ??= [];
        operation.Extensions.Add("X-foo", new JsonNodeExtension("bar"));
        operation.Extensions.Add("X-docName", new JsonNodeExtension(context.DocumentName));
    }

    public Task ApplyAsync(OpenApiOperation operation, OperationFilterContext context, CancellationToken cancellationToken)
    {
        Apply(operation, context);
        return Task.CompletedTask;
    }
}
