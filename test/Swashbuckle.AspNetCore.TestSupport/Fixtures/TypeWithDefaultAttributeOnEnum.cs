using System.ComponentModel;

namespace Swashbuckle.AspNetCore.TestSupport;

public class TypeWithDefaultAttributeOnEnum
{
    [DefaultValue(IntEnum.Value4)]
    public IntEnum EnumWithDefault { get; set; }
    [DefaultValue(new IntEnum[] { IntEnum.Value4 })]
    public IntEnum[] EnumArrayWithDefault { get; set; }
}
