using Microsoft.OpenApi.Models.Interfaces;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IParameterAsyncFilter
{
    Task ApplyAsync(IOpenApiParameter parameter, ParameterFilterContext context, CancellationToken cancellationToken);
}
