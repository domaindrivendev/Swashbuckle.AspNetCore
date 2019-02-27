using System.Runtime.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public enum AnAnnotatedEnum
    {
        [EnumMember(Value = "foo-bar")]
        FooBar,
        [EnumMember(Value = "bar-foo")]
        BarFoo,
        [EnumMember]
        Default
    }
}