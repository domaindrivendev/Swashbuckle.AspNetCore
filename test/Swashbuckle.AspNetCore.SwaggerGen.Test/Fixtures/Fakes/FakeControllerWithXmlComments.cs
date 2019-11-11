using System;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    /// <summary>
    /// Summary for FakeControllerWithXmlComments
    /// </summary>
    /// <response code="default">Description for default response</response>
    public class FakeControllerWithXmlComments
    {
        /// <summary>Summary for ActionWithNoParameters</summary>
        /// <remarks>Remarks for ActionWithNoParameters</remarks>
        public void ActionWithNoParameters()
        { }

        /// <param name="param">Description for param</param>
        public void ActionWithParamTag(string param)
        { }

        /// <param name="param1">Description for param1</param>
        /// <param name="param2">Description for param2</param>
        public void ActionWithMultipleParameters(string param1, int param2)
        { }

        /// <response code="200">Description for 200 response</response>
        /// <response code="400">Description for 400 response</response>
        public int ActionWithResponseTags()
        {
            throw new NotImplementedException();
        }
    }
}
