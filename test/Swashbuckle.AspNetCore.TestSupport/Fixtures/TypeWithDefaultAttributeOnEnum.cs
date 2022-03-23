using System.ComponentModel;

namespace Swashbuckle.AspNetCore.TestSupport
{
    public class TypeWithDefaultAttributeOnEnum
    {
        [DefaultValue(IntEnum.Value4)]
        public IntEnum EnumWithDefault { get; set; }
    }
}
