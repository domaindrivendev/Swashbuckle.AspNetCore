using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class TestParameterFilter : IParameterFilter, IParameterAsyncFilter
{
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
#if NET10_0_OR_GREATER
        parameter.Extensions.Add("X-foo", new OpenApiAny("bar"));
        parameter.Extensions.Add("X-docName", new OpenApiAny(context.DocumentName));
#else
        parameter.Extensions.Add("X-foo", new OpenApiString("bar"));
        parameter.Extensions.Add("X-docName", new OpenApiString(context.DocumentName));
#endif
    }

    public Task ApplyAsync(OpenApiParameter parameter, ParameterFilterContext context, CancellationToken cancellationToken)
    {
        Apply(parameter, context);
        return Task.CompletedTask;
    }
}
