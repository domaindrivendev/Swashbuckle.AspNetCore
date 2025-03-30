using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class TestRequestBodyFilter : IRequestBodyFilter, IRequestBodyAsyncFilter
{
    public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
    {
#if NET10_0_OR_GREATER
        requestBody.Extensions.Add("X-foo", new OpenApiAny("bar"));
        requestBody.Extensions.Add("X-docName", new OpenApiAny(context.DocumentName));
#else
        requestBody.Extensions.Add("X-foo", new OpenApiString("bar"));
        requestBody.Extensions.Add("X-docName", new OpenApiString(context.DocumentName));
#endif
    }

    public Task ApplyAsync(OpenApiRequestBody requestBody, RequestBodyFilterContext context, CancellationToken cancellationToken)
    {
        Apply(requestBody, context);
        return Task.CompletedTask;
    }
}
