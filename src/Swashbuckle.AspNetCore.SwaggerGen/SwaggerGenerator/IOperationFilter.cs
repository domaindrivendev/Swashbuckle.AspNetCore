using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IOperationFilter
{
    void Apply(OpenApiOperation operation, OperationFilterContext context);
}
