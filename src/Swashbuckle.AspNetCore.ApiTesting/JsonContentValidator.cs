using Microsoft.OpenApi;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting;

public sealed class JsonContentValidator : IContentValidator
{
    private readonly JsonValidator _jsonValidator = new();

    public bool CanValidate(string mediaType) => mediaType.Contains("json");

    public void Validate(IOpenApiMediaType mediaTypeSpec, OpenApiDocument openApiDocument, HttpContent content)
    {
        if (mediaTypeSpec?.Schema == null)
        {
            return;
        }

        var instance = JToken.Parse(content.ReadAsStringAsync().Result);
        if (!_jsonValidator.Validate(mediaTypeSpec.Schema, openApiDocument, instance, out IEnumerable<string> errorMessages))
        {
            throw new ContentDoesNotMatchSpecException(string.Join(Environment.NewLine, errorMessages));
        }
    }
}
