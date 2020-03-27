namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    /// <summary>
    /// Summary for FakeControllerWithXmlComments
    /// </summary>
    /// <response code="default">Description for default response</response>
    public class FakeControllerWithXmlComments
    {
        /// <summary>
        /// Summary for ActionWithSummaryAndRemarksTags
        /// </summary>
        /// <remarks>
        /// Remarks for ActionWithSummaryAndRemarksTags
        /// </remarks>
        public void ActionWithSummaryAndRemarksTags()
        { }

        /// <param name="param1">Description for param1</param>
        /// <param name="param2">Description for param2</param>
        public void ActionWithParamTags(string param1, string param2)
        { }

        /// <response code="200">Description for 200 response</response>
        /// <response code="400">Description for 400 response</response>
        public void ActionWithResponseTags()
        { }
    }
}