using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Basic.Swagger
{
    public class AssignRequestBodyVendorExtensions : IRequestBodyFilter
    {
        public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
        {
            requestBody.Extensions.Add("x-purpose", new OpenApiString("test"));
        }
    }
}
