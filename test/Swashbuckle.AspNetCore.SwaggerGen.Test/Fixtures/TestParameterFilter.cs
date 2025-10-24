using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class TestParameterFilter : IParameterFilter, IParameterAsyncFilter
{
    public void Apply(IOpenApiParameter parameter, ParameterFilterContext context)
    {
        if (parameter is OpenApiParameter openApiParameter)
        {
            openApiParameter.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            openApiParameter.Extensions.Add("X-foo", new JsonNodeExtension("bar"));
            openApiParameter.Extensions.Add("X-docName", new JsonNodeExtension(context.DocumentName));
        }
    }

    public Task ApplyAsync(IOpenApiParameter parameter, ParameterFilterContext context, CancellationToken cancellationToken)
    {
        Apply(parameter, context);
        return Task.CompletedTask;
    }
}
