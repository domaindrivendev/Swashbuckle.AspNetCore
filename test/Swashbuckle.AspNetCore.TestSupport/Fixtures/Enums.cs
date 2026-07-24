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
    [Description("Value two description")]
    Two = 2,

    [Description("Value four description")]
    Four = 4,

    Eight = 8,
}

public enum IntEnumWithDisplayDescriptions : int
{
    [Display(Description = "Value two display description")]
    Two = 2,

    [Display(Description = "Value four display description")]
    Four = 4,

    Eight = 8,
}

public enum IntEnumWithXmlComments : int
{
    /// <summary>Value two xml comment</summary>
    Two = 2,

    /// <summary>Value four xml comment</summary>
    Four = 4,

    Eight = 8,
}

public enum IntEnumWithXmlCommentsAndDescriptionAttribute : int
{
    [Description("Attribute wins")]
    /// <summary>Should be ignored</summary>
    Two = 2,

    /// <summary>Value four xml comment</summary>
    Four = 4,

    Eight = 8,
}
