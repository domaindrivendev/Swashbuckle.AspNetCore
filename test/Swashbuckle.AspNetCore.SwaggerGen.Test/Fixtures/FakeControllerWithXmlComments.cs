using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

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
    {
    }

    /// <param name="param1" example="Example for &quot;param1&quot;">Description for param1</param>
    /// <param name="param2" example="http://test.com/?param1=1&amp;param2=2">Description for param2</param>
    public void ActionWithParamTags(string param1, string param2)
    {
    }

    /// <response code="200">Description for 200 response</response>
    /// <response code="400">Description for 400 response</response>
    public void ActionWithResponseTags()
    {
    }

    /// <summary>
    /// An action with a JSON body
    /// </summary>
    /// <param name="name">Parameter from JSON body</param>
    public void PostBody([FromBody] string name)
    {
    }

    /// <summary>
    /// An action with a form body
    /// </summary>
    /// <param name="name">Parameter from form body</param>
    public void PostForm([FromForm] string name)
    {
    }
}
