using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IRequestBodyAsyncFilter
{
    Task ApplyAsync(IOpenApiRequestBody requestBody, RequestBodyFilterContext context, CancellationToken cancellationToken);
}
