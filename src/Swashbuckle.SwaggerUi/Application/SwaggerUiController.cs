using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Mvc;

namespace Swashbuckle.Application
{
    public class SwaggerUiController : Controller
    {
        private IFileProvider _fileProvider;

        public SwaggerUiController()
        {
            _fileProvider = new EmbeddedFileProvider(GetType().Assembly, "bower_components/swagger-ui/dist");
        }

        [HttpGet("swagger/ui/{*assetPath}")]
        public IActionResult Get(string assetPath)
        {
            if (assetPath == "index.html")
                return View("/SwaggerUi/Index.cshtml");

            return EmbeddedAssetFor(assetPath);
        }
 
        private IActionResult EmbeddedAssetFor(string assetPath)
        {
            var fileInfo = _fileProvider.GetFileInfo(assetPath);
            if (!fileInfo.Exists) return HttpNotFound();

            var contentType = ContentTypeMap[assetPath.Split('.').Last()];
            return new FileStreamResult(fileInfo.CreateReadStream(), contentType); 
        }

        private static Dictionary<string, string> ContentTypeMap = new Dictionary<string, string>
        {
            { "html", "text/html" },
            { "css", "text/css" },
            { "js", "text/javascript" },
            { "gif", "image/gif" },
            { "png", "image/png" },
            { "eot", "application/vnd.ms-fontobject" },
            { "woff", "application/font-woff" },
            { "woff2", "application/font-woff2" },
            { "otf", "application/font-sfnt" },
            { "ttf", "application/font-sfnt" },
            { "svg", "image/svg+xml" }
        };
    }
}