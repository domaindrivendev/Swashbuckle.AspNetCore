using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IOperationAsyncFilter
{
    Task ApplyAsync(OpenApiOperation operation, OperationFilterContext context, CancellationToken cancellationToken);
}
