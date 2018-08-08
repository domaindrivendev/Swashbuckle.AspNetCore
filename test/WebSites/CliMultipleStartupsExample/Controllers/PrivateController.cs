using Microsoft.AspNetCore.Mvc;

namespace CliMultipleStartupsExample.Controllers
{
    [Route("privateAPI")]
    public class PrivateController : PrivateApiController
    {
        [HttpPost]
        public Entity Create([FromBody] Entity entity)
        {
            return entity;
        }

        [HttpDelete]
        public void Delete([FromBody] string id) { }
    }
}
