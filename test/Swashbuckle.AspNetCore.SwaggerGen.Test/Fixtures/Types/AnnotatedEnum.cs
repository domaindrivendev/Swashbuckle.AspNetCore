using System.Runtime.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public enum AnnotatedEnum
    {
        [EnumMember(Value = "AE-FOO")]
        Foo,
        [EnumMember(Value = "AE-BAR")]
        Bar
    }
}
