using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IOperationAsyncFilter
{
    Task ApplyAsync(OpenApiOperation operation, OperationFilterContext context, CancellationToken cancellationToken);
}
