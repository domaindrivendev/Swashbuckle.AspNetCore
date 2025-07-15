using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.Swagger;

public interface IAsyncSwaggerProvider
{
    Task<OpenApiDocument> GetSwaggerAsync(
        string documentName,
        string host = null,
        string basePath = null);
}
