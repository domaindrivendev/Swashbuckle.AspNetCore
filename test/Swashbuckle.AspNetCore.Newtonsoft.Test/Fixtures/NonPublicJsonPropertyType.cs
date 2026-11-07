using Newtonsoft.Json;

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
