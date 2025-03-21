using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers;

[Produces("application/json")]
public class FromHeaderParamsController
{
    [HttpGet("country/validate")]
    public IActionResult Get(
        [FromHeader]string accept,
        [FromHeader(Name = "Content-Type")] string contentType,
        [FromHeader] string authorization,
        [FromQuery] string country)
    {
        return new NoContentResult();
    }
}
