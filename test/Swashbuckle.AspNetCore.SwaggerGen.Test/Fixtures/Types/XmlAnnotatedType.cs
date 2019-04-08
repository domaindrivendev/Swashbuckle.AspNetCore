using System;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    /// <summary>
    /// summary for XmlAnnotatedType
    /// </summary>
    public class XmlAnnotatedType
    {
        /// <summary>
        /// summary for BoolProperty
        /// </summary>
        /// <example>true</example>
        public bool BoolProperty { get; set; }

        /// <summary>
        /// summary for IntProperty
        /// </summary>
        /// <example>10</example>
        public int IntProperty { get; set; }

        /// <summary>
        /// summary for LongProperty
        /// </summary>
        /// <example>4294967295</example>
        public long LongProperty { get; set; }

        /// <summary>
        /// summary for FloatProperty
        /// </summary>
        /// <example>1.2</example>
        public float FloatProperty { get; set; }

        /// <summary>
        /// summary for DoubleProperty
        /// </summary>
        /// <example>1.25</example>
        public double DoubleProperty { get; set; }

        /// <summary>
        /// summary for EnumProperty
        /// </summary>
        /// <example>2</example>
        public IntEnum EnumProperty { get; set; }

        /// <summary>
        /// summary for GuidProperty
        /// </summary>
        /// <example>d3966535-2637-48fa-b911-e3c27405ee09</example>
        public Guid GuidProperty { get; set; }

        /// <summary>
        /// summary for StringProperty
        /// </summary>
        /// <example>example for StringProperty</example>
        public string StringProperty { get; set; }

        /// <summary>
        /// summary for BadExampleIntProperty
        /// </summary>
        /// <example>property bad example</example>
        public int BadExampleIntProperty { get; set; }

        /// <summary>
        /// summary for StringField
        /// </summary>
        /// <example>example for StringField</example>
        public string StringField;

        /// <summary>
        /// summary for BoolField
        /// </summary>
        /// <example>true</example>
        public bool BoolField;

        /// <summary>
        /// summary for AcceptsNothing
        /// </summary>
        public void AcceptsNothing()
        { }

        /// <summary>
        /// summary for AcceptsNestedType
        /// </summary>
        /// <param name="param"></param>
        public void AcceptsNestedType(NestedType param)
        { }

        /// <summary>
        /// summary for AcceptsConstructedGenericType
        /// </summary>
        /// <param name="param"></param>
        public void AcceptsConstructedGenericType(KeyValuePair<string, int> param)
        { }

        /// <summary>
        /// summary for AcceptsConstructedOfConstructedGenericType
        /// </summary>
        /// <param name="param"></param>
        public void AcceptsConstructedOfConstructedGenericType(IEnumerable<KeyValuePair<string, int>> param)
        { }

        /// <summary>
        /// summary for AcceptsArrayOfConstructedGenericType
        /// </summary>
        /// <param name="param"></param>
        public void AcceptsArrayOfConstructedGenericType(int?[] param)
        { }

        /// <summary>
        /// summary for NestedType
        /// </summary>
        public class NestedType
        {
            public string Property { get; set; }
        }
    }
}