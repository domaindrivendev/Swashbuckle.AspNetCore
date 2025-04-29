using System.Text.Json;

namespace Swashbuckle.AspNetCore.SwaggerGen.DependencyInjection;

public class SwaggerGenJsonOptions
{
    public JsonSerializerOptions SerializerOptions { get; set; }
}
