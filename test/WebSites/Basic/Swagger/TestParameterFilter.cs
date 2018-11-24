using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;

namespace Basic.Swagger
{
    public class TestParameterFilter : IParameterFilter
    {
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            parameter.Extensions.Add("x-foobar", new OpenApiBoolean(true));
        }
    }
}
