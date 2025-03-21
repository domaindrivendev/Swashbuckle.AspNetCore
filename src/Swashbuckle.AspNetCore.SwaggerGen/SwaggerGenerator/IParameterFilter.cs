using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IParameterFilter
{
    void Apply(OpenApiParameter parameter, ParameterFilterContext context);
}
