using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;

internal class JsonRequiredAnnotatedType
{

#if NET
    [JsonRequired]
#endif
    public string StringWithJsonRequired { get; set; }
}
