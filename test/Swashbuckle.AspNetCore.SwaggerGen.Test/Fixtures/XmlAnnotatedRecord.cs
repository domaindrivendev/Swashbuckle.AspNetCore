using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    /// <summary>
    /// Summary for XmlAnnotatedRecord
    /// </summary>
    /// <param name="BoolProperty" example="true">Summary for BoolProperty</param>
    /// <param name="IntProperty" example="10">Summary for IntProperty</param>
    /// <param name="LongProperty" example="4294967295">Summary for LongProperty</param>
    /// <param name="FloatProperty" example="1.2">Summary for FloatProperty</param>
    /// <param name="DoubleProperty" example="1.25">Summary for DoubleProperty</param>
    /// <param name="DateTimeProperty" example="6/22/2022 12:00:00 AM">Summary for DateTimeProperty</param>
    /// <param name="EnumProperty" example="2">Summary for EnumProperty</param>
    /// <param name="GuidProperty" example="d3966535-2637-48fa-b911-e3c27405ee09">Summary for GuidProperty</param>
    /// <param name="StringPropertyWithNullExample" example="null">Summary for Nullable StringPropertyWithNullExample</param>
    /// <param name="StringProperty" example="Example for StringProperty">Summary for StringProperty</param>
    /// <param name="StringPropertyWithUri" example="https://test.com/a?b=1&amp;c=2">Summary for StringPropertyWithUri</param>
    /// <param name="ObjectProperty" example="{&quot;prop1&quot;: 1, &quot;prop2&quot;: &quot;foobar&quot;}">Summary for ObjectProperty</param>
    public record XmlAnnotatedRecord(
        bool BoolProperty,
        int IntProperty,
        long LongProperty,
        float FloatProperty,
        double DoubleProperty,
        DateTime DateTimeProperty,
        IntEnum EnumProperty,
        Guid GuidProperty,
        string StringPropertyWithNullExample,
        string StringProperty,
        string StringPropertyWithUri,
        object ObjectProperty
        );
}