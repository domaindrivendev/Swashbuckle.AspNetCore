using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Framework.Runtime;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Mvc;

namespace Swashbuckle.Application
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SwaggerUiController : Controller
    {
        private readonly IEnumerable<IFileProvider> _fileProviders;

        public SwaggerUiController(IApplicationEnvironment applicationEnvironment)
        {
            var thisAssembly = GetType().GetTypeInfo().Assembly;
            var customAssetsPath = applicationEnvironment.ApplicationBasePath + "/SwaggerUi";
            _fileProviders = new IFileProvider[]
            {
                new PhysicalFileProvider(customAssetsPath),
                new EmbeddedFileProvider(thisAssembly, "bower_components/swagger-ui/dist")
            };
        }

        [HttpGet]
        public IActionResult GetAsset(string assetPath)
        {
            if (assetPath == "index.html")
                return View("/SwaggerUi/Index.cshtml");

            foreach (var fileProvider in _fileProviders)
            {
                var fileInfo = fileProvider.GetFileInfo(assetPath);
                if (fileInfo.Exists) return CreateFileStreamResult(fileInfo);
            }

            return new HttpNotFoundResult();
        }
 
        private FileStreamResult CreateFileStreamResult(IFileInfo fileInfo)
        {
            var extension = fileInfo.Name.Split('.').Last();
            return new FileStreamResult(fileInfo.CreateReadStream(), ContentTypeMap[extension]); 
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