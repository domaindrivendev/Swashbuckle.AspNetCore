using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IRequestBodyFilter
{
    void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context);
}
