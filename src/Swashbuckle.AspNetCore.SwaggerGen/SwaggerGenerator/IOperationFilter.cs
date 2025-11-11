using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IOperationFilter
{
    void Apply(OpenApiOperation operation, OperationFilterContext context);
}
