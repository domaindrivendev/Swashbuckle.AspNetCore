using Newtonsoft.Json;

namespace Swashbuckle.Fixtures
{
    /// <summary>
    /// summary for XmlAnnotatedType
    /// </summary>
    public class XmlAnnotatedType : XmlAnnotatedTypeBase
    {
        /// <summary>
        /// summary for Property
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// summary for NestedTypeProperty
        /// </summary>
        public NestedType NestedTypeProperty { get; set; }

        public class NestedType
        {
        }
    }

    public abstract class XmlAnnotatedTypeBase
    {
        /// <summary>
        /// summary for BaseProperty
        /// </summary>
        public string BaseProperty { get; set; }
    }
}