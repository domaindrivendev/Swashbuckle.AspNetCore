using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IDocumentFilter
{
    void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context);
}
