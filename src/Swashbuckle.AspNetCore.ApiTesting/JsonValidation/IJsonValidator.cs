using Microsoft.OpenApi;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting;

public interface IJsonValidator
{
    bool CanValidate(IOpenApiSchema schema);

    bool Validate(
        IOpenApiSchema schema,
        OpenApiDocument openApiDocument,
        JToken instance,
        out IEnumerable<string> errorMessages);
}
