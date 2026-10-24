using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.ApiTesting;

public interface IContentValidator
{
    bool CanValidate(string mediaType);

    void Validate(IOpenApiMediaType mediaTypeSpec, OpenApiDocument openApiDocument, HttpContent content);
}
