using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class TestOperationFilter : IOperationFilter, IOperationAsyncFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
#if NET10_0
        operation.Extensions.Add("X-foo", new OpenApiAny("bar"));
        operation.Extensions.Add("X-docName", new OpenApiAny(context.DocumentName));
#else
        operation.Extensions.Add("X-foo", new OpenApiString("bar"));
        operation.Extensions.Add("X-docName", new OpenApiString(context.DocumentName));
#endif
    }

    public Task ApplyAsync(OpenApiOperation operation, OperationFilterContext context, CancellationToken cancellationToken)
    {
        Apply(operation, context);
        return Task.CompletedTask;
    }
}
