using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.FileProviders;

namespace Swashbuckle.Application
{
    public class SwaggerUiHandler : IRouter
    {
        private readonly IRootUrlResolver _rootUrlResolver;
        private readonly IFileProvider _fileProvider;

        public SwaggerUiHandler(IRootUrlResolver rootUrlResolver, IFileProvider fileProvider)
        {
            _rootUrlResolver = rootUrlResolver;
            _fileProvider = fileProvider;
        }

        public async Task RouteAsync(RouteContext context)
        {
            var assetPath = GetAssetPath(context);

            if (assetPath == null)
            {
                RedirectToIndex(context);
                return;
            }

            var fileInfo = _fileProvider.GetFileInfo(assetPath);
            if (!fileInfo.Exists)
            {
                ReturnNotFound(context);
                return;
            }

            await ReturnFileContents(context, fileInfo);
        }

        private string GetAssetPath(RouteContext context)
        {
            object routeValue;
            context.RouteData.Values.TryGetValue("assetPath", out routeValue);
            return (routeValue == null) ? null : routeValue.ToString();
        }

        private void RedirectToIndex(RouteContext context)
        {
            throw new NotImplementedException();
        }

        private void ReturnNotFound(RouteContext context)
        {
            context.HttpContext.Response.StatusCode = 404;
        }

        private Task ReturnFileContents(RouteContext context, IFileInfo fileInfo)
        {
            var response = context.HttpContext.Response;

            response.StatusCode = 200;
            response.ContentType = ContentTypeMap[fileInfo.Name.Split('.').Last()];
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

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            throw new NotImplementedException();
        }
    }
}
