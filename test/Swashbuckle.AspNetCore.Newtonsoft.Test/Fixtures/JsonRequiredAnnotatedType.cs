using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

[DataContract]
public class JsonRequiredAnnotatedType
{
    [JsonRequired]
    public string StringWithJsonRequired { get; set; }

    [DataMember(IsRequired = false)] //As the support for DataMember has been implemented later, JsonRequired should take precedence
    [JsonRequired]
    public string StringWithConflictingRequired { get; set; }
}
