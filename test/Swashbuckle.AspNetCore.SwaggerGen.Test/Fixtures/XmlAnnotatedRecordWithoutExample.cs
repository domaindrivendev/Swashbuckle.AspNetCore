using System;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    /// <summary>
    /// Summary for XmlAnnotatedRecordWithoutExample
    /// </summary>
    /// <param name="BoolProperty">Summary for BoolProperty</param>
    /// <param name="IntProperty">Summary for IntProperty</param>
    /// <param name="LongProperty">Summary for LongProperty</param>
    /// <param name="FloatProperty">Summary for FloatProperty</param>
    /// <param name="DoubleProperty">Summary for DoubleProperty</param>
    /// <param name="DateTimeProperty">Summary for DateTimeProperty</param>
    /// <param name="EnumProperty">Summary for EnumProperty</param>
    /// <param name="GuidProperty">Summary for GuidProperty</param>
    /// <param name="StringPropertyWithNullExample">Summary for Nullable StringPropertyWithNullExample</param>
    /// <param name="StringProperty">Summary for StringProperty</param>
    /// <param name="StringPropertyWithUri">Summary for StringPropertyWithUri</param>
    /// <param name="ObjectProperty">Summary for ObjectProperty</param>
    public record XmlAnnotatedRecordWithoutExample(
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