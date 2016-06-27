using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Model;
using System;

namespace Swashbuckle.Swagger.Application
{
    public class SwaggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISwaggerProvider _swaggerProvider;
        private readonly JsonSerializer _swaggerSerializer;
        private readonly Action<HttpRequest, SwaggerDocument> _documentFilter;
        private readonly TemplateMatcher _requestMatcher;

        public SwaggerMiddleware(
            RequestDelegate next,
            ISwaggerProvider swaggerProvider,
            IOptions<MvcJsonOptions> mvcJsonOptions,
            Action<HttpRequest, SwaggerDocument> documentFilter,
            string routeTemplate)
        {
            _next = next;
            _swaggerProvider = swaggerProvider;
            _swaggerSerializer = SwaggerSerializerFactory.Create(mvcJsonOptions);
            _documentFilter = documentFilter;
            _requestMatcher = new TemplateMatcher(TemplateParser.Parse(routeTemplate), new RouteValueDictionary());
        }

        public async Task Invoke(HttpContext httpContext)
        {
            string apiVersion;
            if (!RequestingSwaggerDocs(httpContext.Request, out apiVersion))
            {
                await _next(httpContext);
                return;
            }

            var basePath = string.IsNullOrEmpty(httpContext.Request.PathBase)
                ? "/"
                : httpContext.Request.PathBase.ToString();

            var swagger = _swaggerProvider.GetSwagger(apiVersion, null, basePath);

            // One last opportunity to modify the Swagger Document - this time with request context
            _documentFilter(httpContext.Request, swagger);

            RespondWithSwaggerJson(httpContext.Response, swagger);
        }

        private bool RequestingSwaggerDocs(HttpRequest request, out string apiVersion)
        {
            apiVersion = null;
            if (request.Method != "GET") return false;

			var routeValues = new RouteValueDictionary();
            if (!_requestMatcher.TryMatch(request.Path, routeValues) || !routeValues.ContainsKey("apiVersion")) return false;

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
