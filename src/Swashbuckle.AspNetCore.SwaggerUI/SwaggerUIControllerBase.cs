using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Swashbuckle.AspNetCore.SwaggerUI
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class SwaggerUIControllerBase : ControllerBase
    {
        public const string DefaultRouteTemplate = "swagger";

        public virtual IActionResult Get()
        {
            var options = GetSwaggerUIOptions();

            var builder = new SwaggerUIIndexHtmlBuilder(options);

            var result = builder.Build();

            return Content(result, "text/html");
        }

        private SwaggerUIOptions GetSwaggerUIOptions()
        {
            var options =
                HttpContext
                    .RequestServices
                    .GetService<IOptions<SwaggerUIOptions>>();

            return options?.Value ?? new SwaggerUIOptions();
        }
    }
}