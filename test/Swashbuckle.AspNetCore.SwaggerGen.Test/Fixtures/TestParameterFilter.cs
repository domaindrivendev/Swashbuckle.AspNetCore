using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models.Interfaces;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class TestParameterFilter : IParameterFilter, IParameterAsyncFilter
{
    public void Apply(IOpenApiParameter parameter, ParameterFilterContext context)
    {
        parameter.Extensions.Add("X-foo", new OpenApiAny("bar"));
        parameter.Extensions.Add("X-docName", new OpenApiAny(context.DocumentName));
    }

    public Task ApplyAsync(IOpenApiParameter parameter, ParameterFilterContext context, CancellationToken cancellationToken)
    {
        Apply(parameter, context);
        return Task.CompletedTask;
    }
}
