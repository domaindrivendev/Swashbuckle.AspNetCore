using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Http;

namespace Swashbuckle.Application
{
    public class SwaggerUiRouteHandler : IRouter
    {
        private readonly IFileProvider _fileProvider;

        public SwaggerUiRouteHandler(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            throw new NotImplementedException();
        }

        public Task RouteAsync(RouteContext context)
        {
            var response = context.HttpContext.Response;
            var assetPath = GetAssetPathFrom(context);

            if (assetPath != null)
            {
                var fileInfo = _fileProvider.GetFileInfo(assetPath);
                if (fileInfo.Exists)
                {
                    response.ContentType = ContentTypeMap[assetPath.Split('.').Last()];
                    return WriteFileAsync(fileInfo, response);
                }
            }

            throw new Exception("Redirect");
        }

        private string GetAssetPathFrom(RouteContext context)
        {
            var routeData = context.RouteData;
            if (routeData != null && routeData.Values != null && routeData.Values.ContainsKey("assetPath"))
                return routeData.Values["assetPath"].ToString();

            return null;
        }

        private Task WriteFileAsync(IFileInfo fileInfo, HttpResponse response)
        {
            return fileInfo.CreateReadStream().CopyToAsync(response.Body);
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
