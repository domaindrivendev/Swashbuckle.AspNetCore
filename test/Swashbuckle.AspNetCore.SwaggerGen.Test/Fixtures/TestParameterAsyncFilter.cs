using System.Threading.Tasks;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class TestParameterAsyncFilter : IParameterAsyncFilter
    {
        public Task ApplyAsync(OpenApiParameter parameter, ParameterFilterContext context)
        {
            parameter.Extensions.Add("X-foo", new OpenApiString("bar"));
            parameter.Extensions.Add("X-docName", new OpenApiString(context.DocumentName));

            return Task.CompletedTask;
        }
    }
}
