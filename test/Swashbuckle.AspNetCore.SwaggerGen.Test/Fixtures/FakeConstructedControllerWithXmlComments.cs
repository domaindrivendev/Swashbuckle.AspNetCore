namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class FakeConstructedControllerWithXmlComments : GenericControllerWithXmlComments<string>
    { }

    /// <summary>
    /// Summary for GenericControllerWithXmlComments
    /// </summary>
    public class GenericControllerWithXmlComments<T>
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
        public void ActionWithSummaryAndResponseTags(T param)
        { }

        /// <param name="param1" example="Example for param1">Description for param1</param>
        /// <param name="param1" example="Пример для param1" xml:lang="ru">Описание для param1</param>
        /// <param name="param2" example="Example for param2">Description for param2</param>
        /// <param name="param2" example="Пример для param2" xml:lang="ru">Описание для param2</param>
        public void ActionWithParamTags(T param1, T param2)
        { }
    }
}
