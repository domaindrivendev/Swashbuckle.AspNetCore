using System.Collections.Generic;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
    [Produces("application/json")]
    public class DynamicTypesController
    {
        [HttpPost("kittens")]
        public int CreateKitten([FromBody]dynamic kitten)
        {
            return 1;
        }

        [HttpGet("unicorns")]
        public ExpandoObject GetUnicorns()
        {
            return new ExpandoObject(); 
        }

        [HttpPost("dragons")]
        public IActionResult CreateDragons([FromBody]object dragon)
        {
            return new ObjectResult(1);
        }
    }
}