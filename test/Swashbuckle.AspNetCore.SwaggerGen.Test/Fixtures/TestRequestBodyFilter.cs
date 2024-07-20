using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class TestRequestBodyFilter : IRequestBodyFilter, IRequestBodyAsyncFilter
    {
        public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
        {
            requestBody.Extensions.Add("X-foo", new OpenApiString("bar"));
            requestBody.Extensions.Add("X-docName", new OpenApiString(context.DocumentName));
        }

        public Task ApplyAsync(OpenApiRequestBody requestBody, RequestBodyFilterContext context, CancellationToken cancellationToken)
        {
            Apply(requestBody, context);
            return Task.CompletedTask;
        }
    }
}
