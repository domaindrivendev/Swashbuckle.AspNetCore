using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.OptionsModel;
using Swashbuckle.Application;

namespace VersionedApi.Swagger
{
    public class SwaggerUiController : Controller
    {
        private readonly IOptions<SwaggerOptions> _optionsAccessor;

        public SwaggerUiController(IOptions<SwaggerOptions> optionsAccessor)
        {
            _optionsAccessor = optionsAccessor;
        }

        [HttpGet("swagger/ui/index.html")]
        [ApiExplorerSettings(IgnoreApi=true)]
        public IActionResult Index()
        {
            return View("~/Swagger/index.cshtml", GetSwaggerPaths());
        }

        private Dictionary<string, string> GetSwaggerPaths()
        {
            return _optionsAccessor.Options.SwaggerGeneratorOptions.ApiVersions.ToDictionary(
                entry => string.Format("{0} Docs", entry.Version.ToUpper()),
                entry => string.Format("{0}/swagger/{1}/swagger.json", Request.PathBase, entry.Version)
            );
        }
    }
}
