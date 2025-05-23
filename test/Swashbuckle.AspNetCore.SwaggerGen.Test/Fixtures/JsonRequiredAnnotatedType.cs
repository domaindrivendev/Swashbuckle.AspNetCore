using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;

internal class JsonRequiredAnnotatedType
{

    [JsonRequired]
    public string StringWithJsonRequired { get; set; }
}
