using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerUIBuilderExtensions
    {
        private const string EmbeddedFileNamespace =
            "Swashbuckle.AspNetCore.SwaggerUI.node_modules.swagger_ui_dist";

        public static IApplicationBuilder UseSwaggerUI(
            this IApplicationBuilder app,
            Action<SwaggerUIOptions> setupAction = null)
        {
            if (setupAction == null)
            {
                // Don't pass options so it can be configured/injected via DI container instead
                app.UseMiddleware<SwaggerUIMiddleware>();
            }
            else
            {
                // Configure an options instance here and pass directly to the middleware
                var options = new SwaggerUIOptions();
                setupAction.Invoke(options);

                app.UseMiddleware<SwaggerUIMiddleware>(options);
            }

            return app;
        }

        public static IApplicationBuilder UseSwaggerUIStaticFiles(
            this IApplicationBuilder app,
            string requestPath = null)
        {
            requestPath = requestPath ?? GetRequestPathFromOptions(app);

            var trimmedRequestPath =
                $"/{requestPath.TrimStart('/').TrimEnd('/')}";

            app.UseStaticFiles(
                new StaticFileOptions
                {
                    RequestPath = trimmedRequestPath,
                    FileProvider = new EmbeddedFileProvider(
                        typeof(SwaggerUIMiddleware).GetTypeInfo().Assembly,
                        EmbeddedFileNamespace),
                });

            return app;
        }

        private static string GetRequestPathFromOptions(IApplicationBuilder app)
        {
            var options =
                app
                    .ApplicationServices
                    .GetService<IOptions<SwaggerUIOptions>>();

            var swaggerUiOptions = options?.Value ?? new SwaggerUIOptions();

            return swaggerUiOptions.BasePath;
        }
    }
}
