namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    /// <summary>
    /// Summary for FakeControllerWithXmlComments
    /// </summary>
    /// <summary xml:lang="ru">
    /// Summary для FakeControllerWithXmlComments с русской локализацией
    /// </summary>
    /// <response code="default">Description for default response</response>
    /// <response code="default" xml:lang="ru">Описание для ответа по умолчанию</response>
    public class FakeControllerWithXmlComments
    {
        /// <summary>
        /// Summary for ActionWithSummaryAndRemarksTags
        /// </summary>
        /// <summary xml:lang="ru">
        /// Summary для ActionWithSummaryAndRemarksTags с русской локализацией
        /// </summary>
        /// <remarks>
        /// Remarks for ActionWithSummaryAndRemarksTags
        /// </remarks>
        /// <remarks xml:lang="ru">
        /// Remarks для ActionWithSummaryAndRemarksTags с русской локализацией
        /// </remarks>
        public void ActionWithSummaryAndRemarksTags()
        { }

        /// <param name="param1" example="Example for param1">Description for param1</param>
        /// <param name="param1" example="Пример для param1" xml:lang="ru">Описание для param1</param>
        /// <param name="param2" example="http://test.com/?param1=1&amp;param2=2">Description for param2</param>
        /// <param name="param2" example="http://test.com/?param1=1&amp;param2=2" xml:lang="ru">Описание для param2</param>
        public void ActionWithParamTags(string param1, string param2)
        { }

        /// <response code="200">Description for 200 response</response>
        /// <response code="200" xml:lang="ru">Описание для 200 ответа</response>
        /// <response code="400">Description for 400 response</response>
        /// <response code="400" xml:lang="ru">Описание для 400 ответа</response>
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