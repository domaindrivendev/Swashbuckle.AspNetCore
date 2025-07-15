using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IDocumentAsyncFilter
{
    Task ApplyAsync(OpenApiDocument swaggerDoc, DocumentFilterContext context, CancellationToken cancellationToken);
}
