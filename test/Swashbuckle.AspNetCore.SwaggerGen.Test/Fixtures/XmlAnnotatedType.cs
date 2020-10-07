﻿using System;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    /// <summary>
    /// Summary for XmlAnnotatedType
    /// </summary>
    /// <remarks>
    /// Remarks for XmlAnnotatedType
    /// </remarks>
    public class XmlAnnotatedType
    {
        /// <summary>
        /// Summary for BoolField
        /// </summary>
        /// <remarks>
        /// Remarks for BoolField
        /// </remarks>
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
        /// Summary for StringProperty
        /// </summary>
        /// <remarks>
        /// Remarks for StringProperty
        /// </remarks>
        /// <example>Example for StringProperty</example>
        public string StringProperty { get; set; }

        /// <summary>
        /// Summary for BadExampleIntProperty
        /// </summary>
        /// <example>Foobar</example>
        public int BadExampleIntProperty { get; set; }

        /// <summary>
        /// Summary for AcceptsNothing
        /// </summary>
        public void AcceptsNothing()
        { }

        /// <summary>
        /// Summary for AcceptsNestedType
        /// </summary>
        /// <param name="param"></param>
        public void AcceptsNestedType(NestedType param)
        { }

        /// <summary>
        /// Summary for AcceptsConstructedGenericType
        /// </summary>
        /// <param name="param"></param>
        public void AcceptsConstructedGenericType(KeyValuePair<string, int> param)
        { }

        /// <summary>
        /// Summary for AcceptsConstructedOfConstructedGenericType
        /// </summary>
        /// <param name="param"></param>
        public void AcceptsConstructedOfConstructedGenericType(IEnumerable<KeyValuePair<string, int>> param)
        { }

        /// <summary>
        /// Summary for AcceptsArrayOfConstructedGenericType
        /// </summary>
        /// <param name="param"></param>
        public void AcceptsArrayOfConstructedGenericType(int?[] param)
        { }

        /// <summary>
        /// Summary for NestedType
        /// </summary>
        /// <remarks>
        /// Remarks for NestedType
        /// </remarks>
        public class NestedType
        {
            public string Property { get; set; }

            public class InnerNestedType
            {
                /// <summary>
                /// Summary of DoubleNestedType.InnerType.Property
                /// </summary>
                public string InnerProperty { get; set; }
            }
        }
    }
}