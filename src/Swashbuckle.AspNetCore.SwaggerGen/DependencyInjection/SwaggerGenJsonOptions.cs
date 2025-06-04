using System.Text.Json;

namespace Swashbuckle.AspNetCore.SwaggerGen.DependencyInjection;

/// <summary>
/// Configures the <see cref="JsonSerializerOptions"/> to be used by <see cref="JsonSerializerDataContractResolver"/>.
/// </summary>
public class SwaggerGenJsonOptions
{
    /// <summary>
    /// Gets or sets the JSON serializer options to use.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; set; }
}
