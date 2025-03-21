using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

/// <summary>
/// Summary for XmlAnnotatedTypeWithoutExample
/// </summary>
public class XmlAnnotatedTypeWithoutExample
{
    /// <summary>
    /// Summary for BoolProperty
    /// </summary>
    public bool BoolProperty { get; set; }

    /// <summary>
    /// Summary for IntProperty
    /// </summary>
    public int IntProperty { get; set; }

    /// <summary>
    /// Summary for LongProperty
    /// </summary>
    public long LongProperty { get; set; }

    /// <summary>
    /// Summary for FloatProperty
    /// </summary>
    public float FloatProperty { get; set; }

    /// <summary>
    /// Summary for DoubleProperty
    /// </summary>
    public double DoubleProperty { get; set; }

    /// <summary>
    /// Summary for DateTimeProperty
    /// </summary>
    public DateTime DateTimeProperty { get; set; }

    /// <summary>
    /// Summary for EnumProperty
    /// </summary>
    public IntEnum EnumProperty { get; set; }

    /// <summary>
    /// Summary for GuidProperty
    /// </summary>
    public Guid GuidProperty { get; set; }

    /// <summary>
    /// Summary for Nullable StringPropertyWithNullExample
    /// </summary>
    public string StringPropertyWithNullExample { get; set; }

    /// <summary>
    /// Summary for StringProperty
    /// </summary>
    public string StringProperty { get; set; }

    /// <summary>
    /// Summary for StringPropertyWithUri
    /// </summary>
    public string StringPropertyWithUri { get; set; }

    /// <summary>
    /// Summary for ObjectProperty
    /// </summary>
    public object ObjectProperty { get; set; }
}
