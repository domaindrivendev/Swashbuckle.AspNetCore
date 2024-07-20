using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;

internal class JsonRequiredAnnotatedType
{

#if NET7_0_OR_GREATER
    [JsonRequired]
#endif
    public string StringWithJsonRequired { get; set; }
}
