using Microsoft.AspNet.Mvc;
using Swashbuckle.Application;

namespace MultipleVersions.Swagger
{
    public class SwaggerUiController : Controller
    {
        private readonly SwaggerPathHelper _swaggerPathHelper;

        public SwaggerUiController(SwaggerPathHelper swaggerPathHelper)
        {
            _swaggerPathHelper = swaggerPathHelper;
        }

        [HttpGet("swagger/ui/index.html")]
        [ApiExplorerSettings(IgnoreApi=true)]
        public IActionResult Index()
        {
            return View("~/Swagger/index.cshtml", _swaggerPathHelper.GetPathDescriptors(Request.PathBase));
        }
    }
}
