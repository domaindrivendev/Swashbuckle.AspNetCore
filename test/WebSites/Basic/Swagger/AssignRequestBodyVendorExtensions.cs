using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Basic.Swagger;

public class AssignRequestBodyVendorExtensions : IRequestBodyFilter
{
    public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
    {
#if NET10_0_OR_GREATER
        requestBody.Extensions.Add("x-purpose", new OpenApiAny("test"));
#else
        requestBody.Extensions.Add("x-purpose", new OpenApiString("test"));
#endif
    }
}
