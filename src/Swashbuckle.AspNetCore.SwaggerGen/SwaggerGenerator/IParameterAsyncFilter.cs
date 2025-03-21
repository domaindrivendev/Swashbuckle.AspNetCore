using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IParameterAsyncFilter
{
    Task ApplyAsync(OpenApiParameter parameter, ParameterFilterContext context, CancellationToken cancellationToken);
}
