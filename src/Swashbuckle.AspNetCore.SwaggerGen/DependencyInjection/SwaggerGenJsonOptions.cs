using System.Text.Json;

namespace Swashbuckle.AspNetCore.SwaggerGen.DependencyInjection;

/// <summary>
/// Configures the <see cref="JsonSerializerOptions"/> to be used by <see cref="JsonSerializerDataContractResolver"/>.
/// </summary>
public class SwaggerGenJsonOptions
{
    /// <summary>
    /// Gets or Sets the json serializer options used by <see cref="JsonSerializerDataContractResolver"/>. 
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; set; }
}
