using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Routing.Template;
using Newtonsoft.Json;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public class SwaggerDocsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISwaggerProvider _swaggerProvider;
        private readonly TemplateMatcher _requestMatcher;
        private readonly JsonSerializer _swaggerSerializer;

        public SwaggerDocsMiddleware(
            RequestDelegate next,
            ISwaggerProvider swaggerProvider,
            string routeTemplate)
        {
            _next = next;
            _swaggerProvider = swaggerProvider;
            _requestMatcher = new TemplateMatcher(TemplateParser.Parse(routeTemplate), new RouteValueDictionary());
            _swaggerSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new SwaggerDocsContractResolver()
            };
        }

        public async Task Invoke(HttpContext httpContext)
        {
            string apiVersion;
            if (!RequestingSwaggerDocs(httpContext.Request, out apiVersion))
            {
                await _next(httpContext);
                return;
            }

            var swagger = _swaggerProvider.GetSwagger(apiVersion, null, httpContext.Request.PathBase);
            RespondWithSwaggerJson(httpContext.Response, swagger);
        }

        private bool RequestingSwaggerDocs(HttpRequest request, out string apiVersion)
        {
            apiVersion = null;
            if (request.Method != "GET") return false;

            var routeValues = _requestMatcher.Match(request.Path);
            if (routeValues == null || !routeValues.ContainsKey("apiVersion")) return false;

            apiVersion = routeValues["apiVersion"].ToString();
            return true;
        }

        private void RespondWithSwaggerJson(HttpResponse response, SwaggerDocument swagger)
        {
            response.StatusCode = 200;
            response.ContentType = "application/json";

            using (var writer = new StreamWriter(response.Body))
            {
                _swaggerSerializer.Serialize(writer, swagger);
            }
        }
    }
}
