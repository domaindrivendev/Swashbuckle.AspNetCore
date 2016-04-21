using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace MultipleVersions.Swagger
{
    public class SwaggerUiController : Controller
    {
        [HttpGet("swagger/ui/index.html")]
        [ApiExplorerSettings(IgnoreApi=true)]
        public IActionResult Index()
        {
            return View("~/Swagger/index.cshtml", GetDiscoveryUrls());
        }

        private IDictionary<string, string> GetDiscoveryUrls()
        {
            return new Dictionary<string, string>()
            {
                { "V1", "/swagger/v1/swagger.json" },
                { "V2", "/swagger/v2/swagger.json" }
            };
        }
    }
}
