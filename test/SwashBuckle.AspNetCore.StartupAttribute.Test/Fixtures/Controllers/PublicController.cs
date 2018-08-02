using Microsoft.AspNetCore.Mvc;

namespace SwashBuckle.AspNetCore.StartupAttribute.Test.Fixtures.Controllers
{
    [Route("public")]
    public class PublicController : Controller
    {
        [HttpGet("{id}")]
        public Entity Load(string id)
        {
            return new Entity() { Name = id };
        }
    }
}
