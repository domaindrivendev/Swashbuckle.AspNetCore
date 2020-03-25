namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class FakeConstructedControllerWithXmlComments : FakeGenericControllerWithXmlComments<string>
    { }

    public class FakeGenericControllerWithXmlComments<T>
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
