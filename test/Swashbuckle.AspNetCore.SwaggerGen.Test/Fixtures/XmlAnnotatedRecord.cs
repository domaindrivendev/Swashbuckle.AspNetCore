using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

#nullable enable
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
/// <param name="NullableStringPropertyWithNullExample" example="null">Summary for NullableStringPropertyWithNullExample</param>
/// <param name="StringPropertyWithNullExample" example="null">Summary for StringPropertyWithNullExample</param>
/// <param name="NullableIntPropertyWithNotNullExample" example="3">Summary for NullableIntPropertyWithNotNullExample</param>
/// <param name="NullableIntPropertyWithNullExample" example="null">Summary for NullableIntPropertyWithNullExample</param>
/// <param name="IntPropertyWithNullExample" example="null">Summary for IntPropertyWithNullExample</param>
/// <param name="NullableGuidPropertyWithNullExample" example="null">Summary for NullableGuidPropertyWithNullExample</param>
/// <param name="GuidPropertyWithNullExample" example="null">Summary for GuidPropertyWithNullExample</param>
/// <param name="StringProperty" example="Example for StringProperty">Summary for StringProperty</param>
/// <param name="StringPropertyWithUri" example="https://test.com/a?b=1&amp;c=2">Summary for StringPropertyWithUri</param>
/// <param name="ObjectProperty" example="{&quot;prop1&quot;: 1, &quot;prop2&quot;: &quot;foobar&quot;}">Summary for ObjectProperty</param>
/// <param name="ObjectPropertyNullExample" example="null">Summary for ObjectPropertyNullExample</param>
/// <param name="NullableObjectPropertyNullExample" example="null">Summary for NullableObjectPropertyNullExample</param>
/// <param name="NullableDateTimePropertyWithNullExample" example="null">Summary for NullableDateTimePropertyWithNullExample</param>
/// <param name="DateTimePropertyWithNullExample" example="null">Summary for DateTimePropertyWithNullExample</param>
/// <param name="NullableTimeOnlyPropertyWithNullExample" example="null">Summary for NullableTimeOnlyPropertyWithNullExample</param>
/// <param name="TimeOnlyPropertyWithNullExample" example="null">Summary for TimeOnlyPropertyWithNullExample</param>
/// <param name="NullableTimeSpanPropertyWithNullExample" example="null">Summary for NullableTimeSpanPropertyWithNullExample</param>
/// <param name="TimeSpanPropertyWithNullExample" example="null">Summary for TimeSpanPropertyWithNullExample</param>
/// <param name="NullableDateOnlyPropertyWithNullExample" example="null">Summary for NullableDateOnlyPropertyWithNullExample</param>
/// <param name="DateOnlyPropertyWithNullExample" example="null">Summary for DateOnlyPropertyWithNullExample</param>
public record XmlAnnotatedRecord(
    bool BoolProperty,
    int IntProperty,
    long LongProperty,
    float FloatProperty,
    double DoubleProperty,
    DateTime DateTimeProperty,
    IntEnum EnumProperty,
    Guid GuidProperty,
    string? NullableStringPropertyWithNullExample,
    string StringPropertyWithNullExample,
    int? NullableIntPropertyWithNotNullExample,
    int? NullableIntPropertyWithNullExample,
    int IntPropertyWithNullExample,
    Guid? NullableGuidPropertyWithNullExample,
    Guid GuidPropertyWithNullExample,
    string StringProperty,
    string StringPropertyWithUri,
    object ObjectProperty,
    object ObjectPropertyNullExample,
    object? NullableObjectPropertyNullExample,
    DateTime? NullableDateTimePropertyWithNullExample,
    DateTime DateTimePropertyWithNullExample,
    TimeOnly? NullableTimeOnlyPropertyWithNullExample,
    TimeOnly TimeOnlyPropertyWithNullExample,
    TimeSpan? NullableTimeSpanPropertyWithNullExample,
    TimeSpan TimeSpanPropertyWithNullExample,
    DateOnly? NullableDateOnlyPropertyWithNullExample,
    DateOnly DateOnlyPropertyWithNullExample
    );
