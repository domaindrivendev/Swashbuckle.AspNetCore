namespace Swashbuckle.AspNetCore.TestSupport
{
    public class ConstructedControllerWithXmlComments : GenericControllerWithXmlComments<string>
    { }

    /// <summary>
    /// Summary for GenericControllerWithXmlComments
    /// </summary>
    public class GenericControllerWithXmlComments<T>
    {
        /// <summary>
        /// Summary for ActionWithGenericTypeParameter
        /// </summary>
        /// <remarks>
        /// Remarks for ActionWithGenericTypeParameter
        /// </remarks>
        /// <param name="param">Description for param</param>
        public void ActionWithGenericTypeParameter(T param)
        { }
    }
}
