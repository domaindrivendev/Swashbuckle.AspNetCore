using Swashbuckle.AspNetCore.TestSupport;
using System;

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

        /// <param name="param1" example="Example for param1">Description for param1</param>
        /// <param name="param2" example="Example for param2">Description for param2</param>
        public void ActionWithParamTags(string param1, string param2)
        { }

        /// <response code="200">Description for 200 response</response>
        /// <response code="400">Description for 400 response</response>
        public void ActionWithResponseTags()
        { }

        ///// <param name="boolParam" example="true"></param>
        ///// <param name="intParam" example="27"></param>
        ///// <param name="longParam" example="4294967296"></param>
        ///// <param name="floatParam" example="1.23"></param>
        ///// <param name="doubleParam" example="1.25"></param>
        ///// <param name="enumParam" example="2"></param>
        ///// <param name="guidParam" example="1edab3d2-311a-4782-9ec9-a70d0478b82f"></param>
        ///// <param name="stringParam" example="Example for StringProperty"></param>
        ///// <param name="badExampleIntParam" example="goodbye"></param>
        //public void ActionWithExampleParams(
        //    bool boolParam,
        //    int intParam,
        //    long longParam,
        //    float floatParam,
        //    double doubleParam,
        //    IntEnum enumParam,
        //    Guid guidParam,
        //    string stringParam,
        //    int badExampleIntParam)
        //{ }
    }
}