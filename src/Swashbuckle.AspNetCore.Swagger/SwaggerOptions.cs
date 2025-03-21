using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.Swagger;

public class SwaggerOptions
{
    internal const string DefaultRouteTemplate = "/swagger/{documentName}/swagger.{extension:regex(^(json|ya?ml)$)}";

    public SwaggerOptions()
    {
        PreSerializeFilters = [];
        OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
    }

    /// <summary>
    /// Sets a custom route for the Swagger JSON/YAML endpoint(s). Must include the {documentName} parameter
    /// </summary>
    public string RouteTemplate { get; set; } = DefaultRouteTemplate;

    /// <summary>
    /// Return Swagger JSON/YAML in the V2.0 format rather than V3.0.
    /// </summary>
    [Obsolete($"This property will be removed in a future version of Swashbuckle.AspNetCore. Use the {nameof(OpenApiVersion)} property instead.")]
    public bool SerializeAsV2
    {
        get => OpenApiVersion == OpenApiSpecVersion.OpenApi2_0;
        set => OpenApiVersion = value ? OpenApiSpecVersion.OpenApi2_0 : OpenApiSpecVersion.OpenApi3_0;
    }

    /// <summary>
    /// Gets or sets the OpenAPI (Swagger) document version to use.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="OpenApiSpecVersion.OpenApi3_0"/>.
    /// </remarks>
    public OpenApiSpecVersion OpenApiVersion { get; set; }

    /// <summary>
    /// Gets or sets an optional custom <see cref="ISwaggerDocumentSerializer"/> implementation to use to serialize Swagger documents.
    /// </summary>
    /// <remarks>For the CLI tool to be able to use this, this needs to be configured for use in the service collection of your application.</remarks>
    public ISwaggerDocumentSerializer CustomDocumentSerializer { get; set; }

    /// <summary>
    /// Actions that can be applied to an OpenApiDocument before it's serialized.
    /// Useful for setting metadata that's derived from the current request.
    /// </summary>
    public List<Action<OpenApiDocument, HttpRequest>> PreSerializeFilters { get; private set; }
}
