using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class TestRequestBodyFilter : IRequestBodyFilter, IRequestBodyAsyncFilter
{
    public void Apply(IOpenApiRequestBody requestBody, RequestBodyFilterContext context)
    {
        if (requestBody is OpenApiRequestBody body)
        {
            body.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            body.Extensions.Add("X-foo", new JsonNodeExtension("bar"));
            body.Extensions.Add("X-docName", new JsonNodeExtension(context.DocumentName));
        }
    }

    public Task ApplyAsync(IOpenApiRequestBody requestBody, RequestBodyFilterContext context, CancellationToken cancellationToken)
    {
        Apply(requestBody, context);
        return Task.CompletedTask;
    }
}
