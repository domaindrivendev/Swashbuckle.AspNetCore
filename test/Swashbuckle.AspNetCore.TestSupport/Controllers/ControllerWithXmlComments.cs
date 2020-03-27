namespace Swashbuckle.AspNetCore.TestSupport
{
    /// <summary>
    /// Summary for ControllerWithXmlComments
    /// </summary>
    /// <response code="default">Description for default response</response>
    public class ControllerWithXmlComments
    {
        /// <summary>
        /// Summary for ActionWithNoParameters
        /// </summary>
        /// <remarks>
        /// Remarks for ActionWithNoParameters
        /// </remarks>
        /// <response code="200">Description for 200 response</response>
        /// <response code="400">Description for 400 response</response>
        public void ActionWithNoParameters()
        { }

        /// <param name="param">Description for param</param>
        public void ActionWithParameter(string param)
        { }
    }
}