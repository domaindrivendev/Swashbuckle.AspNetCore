using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IDocumentAsyncFilter
{
    Task ApplyAsync(OpenApiDocument swaggerDoc, DocumentFilterContext context, CancellationToken cancellationToken);
}
