using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.OpenAPI.Application
{
    public class OpenAPIOptions
    {
        public bool CompabilityLayerActive { get; set; } = false;

        public OpenApiSpecVersion Version { get; set; } = OpenApiSpecVersion.OpenApi3_0;

        public OpenApiFormat Format { get; set; } = OpenApiFormat.Json;

    }
}
