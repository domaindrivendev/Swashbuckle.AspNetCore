using System.Diagnostics;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers;

[Produces("application/json")]
public class DynamicTypesController
{
    [HttpPost("kittens")]
    public int CreateKitten([FromBody] dynamic kitten)
    {
        Debug.Assert(kitten is not null);
        return 1;
    }

    [HttpGet("unicorns")]
    public ExpandoObject GetUnicorns()
    {
        return new ExpandoObject(); 
    }

    [HttpPost("dragons")]
    public IActionResult CreateDragons([FromBody] object dragon)
    {
        Debug.Assert(dragon is not null);
        return new ObjectResult(1);
    }
}
