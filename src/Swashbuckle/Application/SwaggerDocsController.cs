using Microsoft.AspNet.Mvc;
using Microsoft.Framework.OptionsModel;
using Newtonsoft.Json;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SwaggerDocsController : Controller
    {
        private ISwaggerProvider _swaggerProvider;

        public SwaggerDocsController(ISwaggerProvider swaggerProvider, IOptions<SwashbuckleOptions> optionsAccessor)
        {
            _swaggerProvider = swaggerProvider;
        }

        [HttpGet("/swagger/docs/{apiVersion}")]
        [Produces("application/json")]
        public IActionResult Get(string apiVersion)
        {
            var swagger = _swaggerProvider.GetSwagger("http://tempuri.org", apiVersion);

            var objectResult = new ObjectResult(swagger);
            objectResult.Formatters.Add(SwaggerOutputFormatter());

            return objectResult;
        }

        private IOutputFormatter SwaggerOutputFormatter()
        {
            var formatter = new JsonOutputFormatter();
            formatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            return formatter;
        }
    }
}