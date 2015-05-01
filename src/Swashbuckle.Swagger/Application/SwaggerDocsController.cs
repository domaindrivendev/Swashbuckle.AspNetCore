using System;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SwaggerDocsController : Controller
    {
        private Func<HttpRequest, string> _rootUrlResolver;
        private ISwaggerProvider _swaggerProvider;

        public SwaggerDocsController(
            Func<HttpRequest, string> rootUrlResolver,
            ISwaggerProvider swaggerProvider)
        {
            _rootUrlResolver = rootUrlResolver;
            _swaggerProvider = swaggerProvider;
        }

        [HttpGet]
        [Produces("application/json")]
        public IActionResult GetDocs(string apiVersion)
        {
            var rootUrl = _rootUrlResolver(Request);
            var swagger = _swaggerProvider.GetSwagger(rootUrl, apiVersion);

            var objectResult = new ObjectResult(swagger);
            objectResult.Formatters.Add(SwaggerOutputFormatter());

            return objectResult;
        }

        private IOutputFormatter SwaggerOutputFormatter()
        {
            var formatter = new JsonOutputFormatter();
            formatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            formatter.SerializerSettings.ContractResolver = new SwaggerDocumentContractResolver();
            return formatter;
        }
    }
}