using System.Text.Json;

namespace Swashbuckle.AspNetCore.SwaggerUI;

public class SwaggerUIOptions
{
    /// <summary>
    /// Gets or sets a route prefix for accessing the swagger-ui
    /// </summary>
    public string RoutePrefix { get; set; } = "swagger";

    /// <summary>
    /// Gets or sets a Stream function for retrieving the swagger-ui page
    /// </summary>
    public Func<Stream> IndexStream { get; set; } = static () => ResourceHelper.GetEmbeddedResource("index.html");

    /// <summary>
    /// Gets or sets a title for the swagger-ui page
    /// </summary>
    public string DocumentTitle { get; set; } = "Swagger UI";

    /// <summary>
    /// Gets or sets additional content to place in the head of the swagger-ui page
    /// </summary>
    public string HeadContent { get; set; } = "";

    /// <summary>
    /// Gets the JavaScript config object, represented as JSON, that will be passed to the SwaggerUI
    /// </summary>
    public ConfigObject ConfigObject { get; set; } = new();

    /// <summary>
    /// Gets the JavaScript config object, represented as JSON, that will be passed to the initOAuth method
    /// </summary>
    public OAuthConfigObject OAuthConfigObject { get; set; } = new();

    /// <summary>
    /// Gets the interceptor functions that define client-side request/response interceptors
    /// </summary>
    public InterceptorFunctions Interceptors { get; set; } = new();

    /// <summary>
    /// Gets or sets the optional JSON serialization options to use to serialize options to the HTML document.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the path or URL to the Swagger UI JavaScript bundle file.
    /// </summary>
    public string ScriptBundlePath { get; set; } = "./swagger-ui-bundle.js";

    /// <summary>
    /// Gets or sets the path or URL to the Swagger UI JavaScript standalone presets file.
    /// </summary>
    public string ScriptPresetsPath { get; set; } = "./swagger-ui-standalone-preset.js";

    /// <summary>
    /// Gets or sets the path or URL to the Swagger UI CSS file.
    /// </summary>
    public string StylesPath { get; set; } = "./swagger-ui.css";

    /// <summary>
    /// Gets or sets whether to expose the <c><see cref="ConfigObject">ConfigObject</see>.Urls</c> object via an
    /// HTTP endpoint with the URL specified by <see cref="SwaggerDocumentUrlsPath"/>
    /// so that external code can auto-discover all Swagger documents.
    /// </summary>
    public bool ExposeSwaggerDocumentUrlsRoute { get; set; }

    /// <summary>
    /// Gets or sets the relative URL path to the route that exposes the values of the configured <see cref="ConfigObject.Urls"/> values.
    /// </summary>
    public string SwaggerDocumentUrlsPath { get; set; } = "documentUrls";

    /// <summary>
    /// Gets or sets the cache lifetime to use for the SwaggerUI files, if any.
    /// </summary>
    /// <remarks>
    /// The default value is 7 days.
    /// </remarks>
    public TimeSpan? CacheLifetime { get; set; } = TimeSpan.FromDays(7);
}
