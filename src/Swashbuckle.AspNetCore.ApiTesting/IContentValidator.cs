using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.ApiTesting;

public interface IContentValidator
{
    bool CanValidate(string mediaType);

    void Validate(OpenApiMediaType mediaTypeSpec, OpenApiDocument openApiDocument, HttpContent content);
}
