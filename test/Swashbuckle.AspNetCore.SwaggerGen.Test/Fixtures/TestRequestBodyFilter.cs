using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class TestRequestBodyFilter : IRequestBodyFilter, IRequestBodyAsyncFilter
{
    public void Apply(IOpenApiRequestBody requestBody, RequestBodyFilterContext context)
    {
        if (requestBody is OpenApiRequestBody body)
        {
            body.Extensions ??= [];
            body.Extensions.Add("X-foo", new OpenApiAny("bar"));
            body.Extensions.Add("X-docName", new OpenApiAny(context.DocumentName));
        }
    }

    public Task ApplyAsync(IOpenApiRequestBody requestBody, RequestBodyFilterContext context, CancellationToken cancellationToken)
    {
        Apply(requestBody, context);
        return Task.CompletedTask;
    }
}
