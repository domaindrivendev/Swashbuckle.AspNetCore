using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IRequestBodyFilter
{
    void Apply(IOpenApiRequestBody requestBody, RequestBodyFilterContext context);
}
