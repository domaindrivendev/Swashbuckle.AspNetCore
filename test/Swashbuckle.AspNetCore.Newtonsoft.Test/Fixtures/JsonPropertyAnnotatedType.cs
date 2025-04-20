using System.Runtime.Serialization;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

[DataContract]
public class JsonPropertyAnnotatedType
{
    [JsonProperty("string-with-json-property-name")]
    public string StringWithJsonPropertyName { get; set; }

    [JsonProperty(Required = Required.Default)]
    public int IntWithRequiredDefault { get; set; }

    [JsonProperty(Required = Required.Default)]
    public string StringWithRequiredDefault { get; set; }

    [JsonProperty(Required = Required.DisallowNull)]
    public string StringWithRequiredDisallowNull { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string StringWithRequiredAlways { get; set; }

    [JsonProperty(Required = Required.AllowNull)]
    public string StringWithRequiredAllowNull { get; set; }

    [DataMember(IsRequired = false)] //As the support for DataMember has been implemented later, JsonProperty.Required should take precedence
    [JsonProperty(Required = Required.Always)]
    public string StringWithRequiredAlwaysButConflictingDataMember { get; set; }

    [DataMember(IsRequired = true)] //As the support for DataMember has been implemented later, JsonProperty.Required should take precedence
    [JsonProperty(Required = Required.Default)]
    public string StringWithRequiredDefaultButConflictingDataMember { get; set; }

    [JsonProperty(Required = Required.Default)]
    public IntEnum? IntEnumWithRequiredDefault { get; set; }

    [JsonProperty(Required = Required.DisallowNull)]
    public IntEnum? IntEnumWithRequiredDisallowNull { get; set; }

    [JsonProperty(Required = Required.Always)]
    public IntEnum? IntEnumWithRequiredAlways { get; set; }

    [JsonProperty(Required = Required.AllowNull)]
    public IntEnum? IntEnumWithRequiredAllowNull { get; set; }
}
