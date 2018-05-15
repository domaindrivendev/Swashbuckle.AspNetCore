using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Swagger;

namespace Basic.Swagger
{
    public class TestParameterFilter : IParameterFilter
    {
        public void Apply(IParameter parameter, ParameterFilterContext context)
        {
            parameter.Extensions.Add("x-foobar", true);
        }
    }
}
