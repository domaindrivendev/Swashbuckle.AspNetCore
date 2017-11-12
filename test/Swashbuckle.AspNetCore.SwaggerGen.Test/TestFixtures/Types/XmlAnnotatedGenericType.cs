namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    /// <summary>
    /// summary for XmlAnnotatedGenericType
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class XmlAnnotatedGenericType<T>
    {
        /// <summary>
        /// Summary for GenericProperty
        /// </summary>
        public T GenericProperty { get; set; }
    }
}