using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Basic.Swagger;

public class AssignRequestBodyVendorExtensions : IRequestBodyFilter
{
    public void Apply(IOpenApiRequestBody requestBody, RequestBodyFilterContext context)
    {
        if (requestBody is OpenApiRequestBody body)
        {
            body.Extensions ??= [];
            body.Extensions.Add("x-purpose", new OpenApiAny("test"));
        }
    }
}
