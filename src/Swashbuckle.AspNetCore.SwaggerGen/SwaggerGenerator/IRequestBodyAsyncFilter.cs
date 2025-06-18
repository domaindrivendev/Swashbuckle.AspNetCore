using Microsoft.OpenApi.Models.Interfaces;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IRequestBodyAsyncFilter
{
    Task ApplyAsync(IOpenApiRequestBody requestBody, RequestBodyFilterContext context, CancellationToken cancellationToken);
}
