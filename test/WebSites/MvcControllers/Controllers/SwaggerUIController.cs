using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace MvcControllers.Controllers
{
    /// <summary>
    /// Summary for SwaggerControllerInheritedController
    /// </summary>
    public class SwaggerUIController : SwaggerUIControllerBase
    {
        [HttpGet(DefaultRouteTemplate)]
        public override IActionResult Get()
        {
            return base.Get();
        }
    }
}