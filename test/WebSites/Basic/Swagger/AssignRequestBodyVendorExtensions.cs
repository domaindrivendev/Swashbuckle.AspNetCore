using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Basic.Swagger;

public class AssignRequestBodyVendorExtensions : IRequestBodyFilter
{
    public void Apply(IOpenApiRequestBody requestBody, RequestBodyFilterContext context)
    {
        if (requestBody is OpenApiRequestBody body)
        {
            body.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            body.Extensions.Add("x-purpose", new JsonNodeExtension("test"));
        }
    }
}
