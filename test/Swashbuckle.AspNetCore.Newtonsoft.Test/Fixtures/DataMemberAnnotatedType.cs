using System.Runtime.Serialization;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

[DataContract(Name = "CustomNameFromDataContract")]
public class DataMemberAnnotatedType
{
    [DataMember(IsRequired = true)]
    public string StringWithDataMemberRequired { get; set; }

    [DataMember(IsRequired = false)]
    public string StringWithDataMemberNonRequired { get; set; }

    [DataMember(IsRequired = true, Name = "RequiredWithCustomNameFromDataMember")]
    public string StringWithDataMemberRequiredWithCustomName { get; set; }

    [DataMember(IsRequired = false, Name = "NonRequiredWithCustomNameFromDataMember")]
    public string StringWithDataMemberNonRequiredWithCustomName { get; set; }
}
