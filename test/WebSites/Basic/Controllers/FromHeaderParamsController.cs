using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers;

[Produces("application/json")]
public class FromHeaderParamsController
{
    [HttpGet("country/validate")]
    public IActionResult Get(
        [FromHeader] string accept,
        [FromHeader(Name = "Content-Type")] string contentType,
        [FromHeader] string authorization,
        [FromQuery] string country)
    {
        Debug.Assert(accept is not null);
        Debug.Assert(contentType is not null);
        Debug.Assert(authorization is not null);
        Debug.Assert(country is not null);

        return new NoContentResult();
    }
}
