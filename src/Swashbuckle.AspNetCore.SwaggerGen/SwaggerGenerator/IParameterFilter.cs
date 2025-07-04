using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IParameterFilter
{
    void Apply(IOpenApiParameter parameter, ParameterFilterContext context);
}
