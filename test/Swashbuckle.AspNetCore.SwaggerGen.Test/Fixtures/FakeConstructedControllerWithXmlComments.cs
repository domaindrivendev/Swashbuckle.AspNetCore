namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class FakeConstructedControllerWithXmlComments : GenericControllerWithXmlComments<string>
{
}

/// <summary>
/// Summary for GenericControllerWithXmlComments
/// </summary>
public class GenericControllerWithXmlComments<T>
{
    /// <summary>
    /// Summary for ActionWithSummaryAndRemarksTags
    /// </summary>
    /// <remarks>
    /// Remarks for ActionWithSummaryAndRemarksTags
    /// </remarks>
    public void ActionWithSummaryAndResponseTags(T param)
    {
    }

    /// <param name="param1" example="Example for &quot;param1&quot;">Description for param1</param>
    /// <param name="param2" example="http://test.com/?param1=1&amp;param2=2">Description for param2</param>
    /// <param name="param3" example="">Description for param3 with empty example</param>
    /// <param name="param4"></param>
    public void ActionWithParamTags(T param1, T param2, T param3, T param4)
    {
    }
}
