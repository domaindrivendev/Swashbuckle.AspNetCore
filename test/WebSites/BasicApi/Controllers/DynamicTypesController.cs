using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json.Linq;

namespace BasicApi.Controllers
{
    [Produces("application/json")]
    public class DynamicTypesController
    {
        [HttpPost("kittens")]
        public int CreateKitten([FromBody]dynamic kitten)
        {
            throw new NotImplementedException();
        }

        [HttpGet("unicorns")]
        public ExpandoObject GetUnicorns()
        {
            throw new NotImplementedException();
        }

        [HttpPost("dragons")]
        public IActionResult CreateDragons([FromBody]object dragon)
        {
            throw new NotImplementedException();
        }

        [HttpGet("wizards")]
        public IEnumerable<JObject> GetWizards()
        {
            throw new NotImplementedException();
        }
    }
}