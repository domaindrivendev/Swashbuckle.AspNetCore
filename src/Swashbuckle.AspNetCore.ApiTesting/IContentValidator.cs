using Microsoft.OpenApi;

#if NET11_0_OR_GREATER
using OpenApiMediaType = Microsoft.OpenApi.IOpenApiMediaType;
#endif

namespace Swashbuckle.AspNetCore.ApiTesting;

public interface IContentValidator
{
    bool CanValidate(string mediaType);

    void Validate(OpenApiMediaType mediaTypeSpec, OpenApiDocument openApiDocument, HttpContent content);
}
