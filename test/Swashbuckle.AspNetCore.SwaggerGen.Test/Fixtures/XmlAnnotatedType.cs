using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;
#nullable enable
/// <summary>
/// Summary for XmlAnnotatedType
/// </summary>
public class XmlAnnotatedType
{
    /// <summary>
    /// Summary for BoolField
    /// </summary>
    /// <example>true</example>
    public bool BoolField;

    /// <summary>
    /// Summary for BoolProperty
    /// </summary>
    /// <example>true</example>
    public bool BoolProperty { get; set; }

    /// <summary>
    /// Summary for IntProperty
    /// </summary>
    /// <example>10</example>
    public int IntProperty { get; set; }

    /// <summary>
    /// Summary for LongProperty
    /// </summary>
    /// <example>4294967295</example>
    public long LongProperty { get; set; }

    /// <summary>
    /// Summary for FloatProperty
    /// </summary>
    /// <example>1.2</example>
    public float FloatProperty { get; set; }

    /// <summary>
    /// Summary for DoubleProperty
    /// </summary>
    /// <example>1.25</example>
    public double DoubleProperty { get; set; }

    /// <summary>
    /// Summary for DateTimeProperty
    /// </summary>
    /// <example>6/22/2022 12:00:00 AM</example>
    public DateTime DateTimeProperty { get; set; }

    /// <summary>
    /// Summary for EnumProperty
    /// </summary>
    /// <example>2</example>
    public IntEnum EnumProperty { get; set; }

    /// <summary>
    /// Summary for GuidProperty
    /// </summary>
    /// <example>d3966535-2637-48fa-b911-e3c27405ee09</example>
    public Guid GuidProperty { get; set; }

    /// <summary>
    /// Summary for NullableStringPropertyWithNullExample
    /// </summary>
    /// <example>null</example>
    public string? NullableStringPropertyWithNullExample { get; set; }

    /// <summary>
    /// Summary for StringPropertyWithNullExample
    /// </summary>
    /// <example>null</example>
    public required string StringPropertyWithNullExample { get; set; }

    /// <summary>
    /// Summary for StringProperty
    /// </summary>
    /// <example>Example for StringProperty</example>
    public required string StringProperty { get; set; }

    /// <summary>
    /// Summary for StringPropertyWithUri
    /// </summary>
    /// <example><![CDATA[https://test.com/a?b=1&c=2]]></example>
    public required string StringPropertyWithUri { get; set; }

    /// <summary>
    /// Summary for ObjectProperty
    /// </summary>
    /// <example>{"prop1": 1, "prop2": "foobar"}</example>
    public required object ObjectProperty { get; set; }

    /// <summary>
    /// Summary for ObjectPropertyNullExample
    /// </summary>
    /// <example>null</example>
    public required object ObjectPropertyNullExample { get; set; }

    /// <summary>
    /// Summary for NullableObjectPropertyNullExample
    /// </summary>
    /// <example>null</example>
    public object? NullableObjectPropertyNullExample { get; set; }

    /// <summary>
    /// Summary for AcceptsNothing
    /// </summary>
    public void AcceptsNothing()
    {
    }

    /// <summary>
    /// Summary for AcceptsNestedType
    /// </summary>
    /// <param name="param"></param>
    public void AcceptsNestedType(NestedType param)
    {
    }

    /// <summary>
    /// Summary for AcceptsConstructedGenericType
    /// </summary>
    /// <param name="param"></param>
    public void AcceptsConstructedGenericType(KeyValuePair<string, int> param)
    {
    }

    /// <summary>
    /// Summary for AcceptsConstructedOfConstructedGenericType
    /// </summary>
    /// <param name="param"></param>
    public void AcceptsConstructedOfConstructedGenericType(
        IEnumerable<KeyValuePair<string, int>> param)
    {
    }

    /// <summary>
    /// Summary for AcceptsArrayOfConstructedGenericType
    /// </summary>
    /// <param name="param"></param>
    public void AcceptsArrayOfConstructedGenericType(int?[] param)
    {
    }
    ///
    /// <summary>
    /// >Summary for NullableIntPropertyWithNotNullExample
    /// </summary>
    /// <example>3</example>
    public int? NullableIntPropertyWithNotNullExample { get; set; }

    /// <summary>
    /// Summary for NullableIntPropertyWithNullExample
    /// </summary>
    /// <example>null</example>
    public int? NullableIntPropertyWithNullExample { get; set; }

    /// <summary>
    /// Summary for IntPropertyWithNullExample
    /// </summary>
    /// <example>null</example>

    public int IntPropertyWithNullExample { get; set; }

    /// <summary>
    /// Summary for NullableGuidPropertyWithNullExample
    /// </summary>
    /// <example>null</example>

    public Guid? NullableGuidPropertyWithNullExample { get; set; }

    /// <summary>
    /// Summary for GuidPropertyWithNullExample
    /// </summary>
    /// <example>null</example>

    public Guid GuidPropertyWithNullExample { get; set; }

    /// <summary>
    /// Summary for NullableDateTimePropertyWithNullExample
    /// </summary>
    /// <example>null</example>

    public DateTime? NullableDateTimePropertyWithNullExample { get; set; }

    /// <summary>
    /// Summary for DateTimePropertyWithNullExample
    /// </summary>
    /// <example>null</example>

    public DateTime DateTimePropertyWithNullExample { get; set; }

    /// <summary>
    /// Summary for NullableTimeOnlyPropertyWithNullExample
    /// </summary>
    /// <example>null</example>

    public TimeOnly? NullableTimeOnlyPropertyWithNullExample { get; set; }

    /// <summary>
    /// Summary for TimeOnlyPropertyWithNullExample
    /// </summary>
    /// <example>null</example>

    public TimeOnly TimeOnlyPropertyWithNullExample { get; set; }

    /// <summary>
    /// Summary for NullableTimeSpanPropertyWithNullExample
    /// </summary>
    /// <example>null</example>

    public TimeSpan? NullableTimeSpanPropertyWithNullExample { get; set; }

    /// <summary>
    /// Summary for TimeSpanPropertyWithNullExample
    /// </summary>
    /// <example>null</example>

    public TimeSpan TimeSpanPropertyWithNullExample { get; set; }

    /// <summary>
    /// Summary for NullableDateOnlyPropertyWithNullExample
    /// </summary>
    /// <example>null</example>

    public DateOnly? NullableDateOnlyPropertyWithNullExample { get; set; }

    /// <summary>
    /// Summary for DateOnlyPropertyWithNullExample
    /// </summary>
    /// <example>null</example>

    public DateOnly DateOnlyPropertyWithNullExample { get; set; }

    /// <summary>
    /// Summary for NestedType
    /// </summary>
    public class NestedType
    {
        public required string Property { get; set; }

        public class InnerNestedType
        {
            /// <summary>
            /// Summary of DoubleNestedType.InnerType.Property
            /// </summary>
            public required string InnerProperty { get; set; }
        }
    }
}
