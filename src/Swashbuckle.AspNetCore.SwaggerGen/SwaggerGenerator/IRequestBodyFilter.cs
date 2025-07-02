using Microsoft.OpenApi.Models.Interfaces;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IRequestBodyFilter
{
    void Apply(IOpenApiRequestBody requestBody, RequestBodyFilterContext context);
}
