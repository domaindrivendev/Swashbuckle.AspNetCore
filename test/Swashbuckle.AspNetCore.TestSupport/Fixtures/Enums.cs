using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Swashbuckle.AspNetCore.TestSupport;

public enum ByteEnum : byte
{
    Value2 = 2,
    Value4 = 4,
    Value8 = 8,
}

public enum ShortEnum : short
{
    Value2 = 2,
    Value4 = 4,
    Value8 = 8,
}

public enum EmptyIntEnum : int
{
}

public enum IntEnum : int
{
    Value2 = 2,
    Value4 = 4,
    Value8 = 8,
}

public enum LongEnum : long
{
    Value2 = 2,
    Value4 = 4,
    Value8 = 8,
}

public enum IntEnumWithDuplicateValues : int
{
    Unknown = 0,
    PreferredName = 1,
    OldNameForBackwardsCompatibility = PreferredName,
}

public enum IntEnumWithDescriptions : int
{
    [Description("Value zero description")]
    Zero = 0,

    [Description("Value one description")]
    One = 1,

    Two = 2,
}

public enum IntEnumWithDisplayDescriptions : int
{
    [Display(Description = "Value zero display description")]
    Zero = 0,

    [Display(Description = "Value one display description")]
    One = 1,

    Two = 2,
}
