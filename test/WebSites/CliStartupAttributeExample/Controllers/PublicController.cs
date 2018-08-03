using Microsoft.AspNetCore.Mvc;

namespace CliStartupAttributeExample.Controllers
{
    [Route("publicAPI")]
    public class PublicController : Controller
    {
        [HttpGet("{id}")]
        public Entity Load(string id)
        {
            return new Entity() { Name = id };
        }
    }
}
