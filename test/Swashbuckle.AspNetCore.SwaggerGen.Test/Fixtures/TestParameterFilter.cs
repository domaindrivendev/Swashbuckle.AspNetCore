using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class TestParameterFilter : IParameterFilter
    {
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            parameter.Extensions.Add("X-foo", new OpenApiString("bar"));
            parameter.Extensions.Add("X-docName", new OpenApiString(context.DocumentName));
        }
    }
}