using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.Swagger;

public interface ISwaggerProvider
{
    OpenApiDocument GetSwagger(
        string documentName,
        string host = null,
        string basePath = null);
}
