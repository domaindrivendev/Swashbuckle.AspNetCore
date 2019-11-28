using System.Runtime.Serialization;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    public enum AnnotatedEnum
    {
        [EnumMember(Value = "AE-FOO")]
        Foo,
        [EnumMember(Value = "AE-BAR")]
        Bar
    }
}
