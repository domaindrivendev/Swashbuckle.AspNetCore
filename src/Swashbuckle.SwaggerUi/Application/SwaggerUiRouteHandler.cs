using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.WebUtilities;

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

        public async Task RouteAsync(RouteContext context)
        {
            var response = context.HttpContext.Response;
            var assetPath = GetAssetPathFrom(context);

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

            await WriteFileContentsAsync(context, fileInfo);
        }

        private string GetAssetPathFrom(RouteContext context)
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
            context.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        }

        private Task WriteFileContentsAsync(RouteContext context, IFileInfo fileInfo)
        {
            var extension = fileInfo.Name.Split('.').Last();
            var response = context.HttpContext.Response;
            response.ContentType = ContentTypeMap[extension];
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
