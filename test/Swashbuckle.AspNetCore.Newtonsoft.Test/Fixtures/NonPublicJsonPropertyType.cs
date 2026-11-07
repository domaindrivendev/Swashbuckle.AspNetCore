using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

public class NonPublicJsonPropertyBaseType
{
    [JsonProperty("basePrivateProperty")]
    private string BasePrivateProperty { get; set; }
}

public class NonPublicJsonPropertyType : NonPublicJsonPropertyBaseType
{
    [JsonProperty("privateProperty")]
    private string PrivateProperty { get; set; }

    [JsonProperty("internalField")]
    internal string InternalField = "value";
}

public class NonPublicAnnotatedJsonPropertyType
{
    [JsonProperty("ignored"), SwaggerIgnore]
    private string Ignored { get; set; }

    [JsonProperty("ranged"), Range(1, 10)]
    private int Ranged { get; set; }

    [JsonProperty("required"), Required]
    private string RequiredProperty { get; set; }

    [JsonProperty("staticRanged"), Range(1, 10)]
    private static int StaticRanged { get; set; }
}
