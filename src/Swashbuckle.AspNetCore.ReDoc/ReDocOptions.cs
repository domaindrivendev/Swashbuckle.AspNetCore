using System.Text.Json;

namespace Swashbuckle.AspNetCore.ReDoc;

public class ReDocOptions
{
    /// <summary>
    /// Gets or sets a route prefix for accessing the redoc page
    /// </summary>
    public string RoutePrefix { get; set; } = "api-docs";

    /// <summary>
    /// Gets or sets a Stream function for retrieving the redoc page
    /// </summary>
    public Func<Stream> IndexStream { get; set; } = static () => ResourceHelper.GetEmbeddedResource("index.html");

    /// <summary>
    /// Gets or sets a title for the redoc page
    /// </summary>
    public string DocumentTitle { get; set; } = "API Docs";

    /// <summary>
    /// Gets or sets additional content to place in the head of the redoc page
    /// </summary>
    public string HeadContent { get; set; } = "";

    /// <summary>
    /// Gets or sets the Swagger JSON endpoint. Can be fully-qualified or relative to the redoc page
    /// </summary>
    public string SpecUrl { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ConfigObject"/> to use.
    /// </summary>
    public ConfigObject ConfigObject { get; set; } = new();

    /// <summary>
    /// Gets or sets the optional <see cref="System.Text.Json.JsonSerializerOptions"/> to use.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; }
}
