using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Swagger;

namespace MvcControllers.Controllers
{
    /// <summary>
    /// Summary for SwaggerControllerInheritedController
    /// </summary>
    public class SwaggerController : SwaggerControllerBase
    {
        [HttpGet(DefaultRouteTemplate)]
        public override IActionResult Get(string documentName)
        {
            return base.Get(documentName);
        }
    }
}