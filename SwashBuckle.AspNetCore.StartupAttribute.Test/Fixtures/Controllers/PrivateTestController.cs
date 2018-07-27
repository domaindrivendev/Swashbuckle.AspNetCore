using Microsoft.AspNetCore.Mvc;

namespace SwashBuckle.AspNetCore.StartupAttribute.Test.Fixtures.Controllers
{
    [Route("private")]
    public class PrivateController : PrivateApiController
    {
        [HttpPost]
        public Entity Create([FromBody] Entity plugin)
        {
            return plugin;
        }

        [HttpDelete]
        public void Delete([FromBody] string id)
        {
            ;
        }
    }
}
