using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Http;
using Newtonsoft.Json;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public class SwaggerDocsHandler : IRouter
    {
        private Func<HttpRequest, string> _rootUrlResolver;
        private ISwaggerProvider _swaggerProvider;

        public SwaggerDocsHandler(
            Func<HttpRequest, string> rootUrlResolver,
            ISwaggerProvider swaggerProvider)
        {
            _rootUrlResolver = rootUrlResolver;
            _swaggerProvider = swaggerProvider;
        }

        public Task RouteAsync(RouteContext context)
        {
            var rootUrl = _rootUrlResolver(context.HttpContext.Request);
            var apiVersion = GetApiVersion(context);

            var swagger = _swaggerProvider.GetSwagger(rootUrl, apiVersion);

            ReturnSwaggerJson(context.HttpContext.Response, swagger);
            return Task.FromResult(0);
        }

        private string GetApiVersion(RouteContext context)
        {
            object routeValue;
            context.RouteData.Values.TryGetValue("apiVersion", out routeValue);
            return (routeValue == null) ? null : routeValue.ToString();
        }

        private void ReturnSwaggerJson(HttpResponse response, SwaggerDocument swaggerDoc)
        {
            response.StatusCode = 200;
            response.ContentType = "application/json";
            using (var writer = new StreamWriter(response.Body))
            {
                SwaggerSerializer().Serialize(writer, swaggerDoc);
            }
        }

        private JsonSerializer SwaggerSerializer()
        {
            return new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new SwaggerDocumentContractResolver()
            };
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            throw new NotImplementedException();
        }
    }
}
