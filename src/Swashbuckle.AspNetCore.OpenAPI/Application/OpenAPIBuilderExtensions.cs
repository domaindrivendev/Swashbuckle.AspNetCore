using System;
using Swashbuckle.AspNetCore.OpenAPI.Application;
using Swashbuckle.AspNetCore.Swagger;

namespace Microsoft.AspNetCore.Builder
{
    public static class OpenAPIBuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerWithOpenAPILayer(
            this IApplicationBuilder app,
            Action<SwaggerOptions> setupAction = null, Action<OpenAPIOptions> setupOpenAPIAction = null)
        {
            if (setupAction == null)
            {
                // Don't pass options so it can be configured/injected via DI container instead
                app.UseMiddleware<SwaggerMiddleware>();
            }
            else
            {
                // Configure an options instance here and pass directly to the middleware
                var options = new SwaggerOptions();
                setupAction.Invoke(options);

                if(setupOpenAPIAction != null)
                {
                    var optionsOpenAPI = new OpenAPIOptions();
                    setupOpenAPIAction.Invoke(optionsOpenAPI);

                    app.UseMiddleware<OpenAPIMiddleware>(options, optionsOpenAPI);
                }
                else
                {
                    app.UseMiddleware<SwaggerMiddleware>(options);
                }
            }

            return app;
        }
    }
}
