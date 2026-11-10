using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

public class NonPublicJsonPropertyBaseType
{
    public string BasePublicProperty { get; set; }

    [JsonProperty("basePrivateProperty")]
    private string BasePrivateProperty { get; set; }

    private string BaseUnmarkedPrivateProperty { get; set; }
}

public class NonPublicJsonPropertyType : NonPublicJsonPropertyBaseType
{
    public string PublicProperty { get; set; }

    [JsonProperty("privateProperty")]
    private string PrivateProperty { get; set; }

    [JsonProperty("internalField")]
    internal string InternalField = "value";

    private string UnmarkedPrivateProperty { get; set; }

    internal string UnmarkedInternalProperty { get; set; }
}
