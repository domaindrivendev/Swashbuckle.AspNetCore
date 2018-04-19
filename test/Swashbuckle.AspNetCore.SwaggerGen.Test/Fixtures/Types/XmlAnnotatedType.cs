using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    /// <summary>
    /// summary for XmlAnnotatedType
    /// </summary>
    public class XmlAnnotatedType
    {
        /// <summary>
        /// summary for Property
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// summary for Field
        /// </summary>
        public string Field;

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