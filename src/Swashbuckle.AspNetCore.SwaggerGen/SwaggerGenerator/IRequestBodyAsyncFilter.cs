using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IRequestBodyAsyncFilter
{
    Task ApplyAsync(OpenApiRequestBody requestBody, RequestBodyFilterContext context, CancellationToken cancellationToken);
}
