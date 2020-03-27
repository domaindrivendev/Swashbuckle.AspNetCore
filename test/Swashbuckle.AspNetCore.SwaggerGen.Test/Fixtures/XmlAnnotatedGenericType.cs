using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    /// <summary>
    /// Summary for XmlAnnotatedGenericType
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    public class XmlAnnotatedGenericType<T,K>
    {
        /// <summary>
        /// Summary for GenericProperty
        /// </summary>
        public T GenericProperty { get; set; }

        /// <summary>
        /// Summary of AcceptsTypeParameters
        /// </summary>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <param name="param3"></param>
        public void AcceptsTypeParameters(int param1, T param2, K param3)
        { }

        /// <summary>
        /// Summary of AcceptsConstructedOfTypeParametersType
        /// </summary>
        /// <param name="param1"></param>
        public void AcceptsConstructedOfTypeParametersType(IDictionary<T, K> param1)
        { }
    }
}