using System;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.Swagger
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class SwaggerControllerBase : ControllerBase
    {
        public const string DefaultRouteTemplate =
            SwaggerOptions.DefaultRouteTemplate;

        public virtual IActionResult Get(string documentName)
        {
            var swaggerProvider = GetSwaggerProvider();
            var options = GetSwaggerOptions();

            var builder = new SwaggerDocumentBuilder(options);

            var result = builder.Build(
                HttpContext.Request,
                swaggerProvider,
                documentName);

            return Content(result, "application/json");
        }

        private ISwaggerProvider GetSwaggerProvider()
        {
            var provider =
                HttpContext
                    .RequestServices
                    .GetService(typeof(ISwaggerProvider))
                    as ISwaggerProvider;

            if (provider == null)
            {
                throw new InvalidOperationException(
                    $"Failed to resolve type '{nameof(ISwaggerProvider)}' from services.");
            }

            return provider;
        }

        private SwaggerOptions GetSwaggerOptions()
        {
            var options =
                HttpContext
                    .RequestServices
                    .GetService(typeof(SwaggerOptions))
                    as SwaggerOptions;

            return options ?? new SwaggerOptions();
        }
    }
}
