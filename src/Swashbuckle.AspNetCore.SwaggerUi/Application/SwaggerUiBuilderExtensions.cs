using System;
using System.Reflection;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.AspNetCore.SwaggerUi;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerUiBuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerUi(
            this IApplicationBuilder app,
            Action<SwaggerUiOptions> setupAction)
        {
            var options = new SwaggerUiOptions();
            setupAction?.Invoke(options);

            // Serve static assets (CSS, JavaScript etc.) via static file server
            var fileServerOptions = new FileServerOptions
            {
                RequestPath = $"/{options.RoutePrefix}",
                EnableDefaultFiles = true,
                FileProvider = new EmbeddedFileProvider(typeof(SwaggerUiBuilderExtensions).GetTypeInfo().Assembly,
                    "Swashbuckle.AspNetCore.SwaggerUi.bower_components.swagger_ui.dist")
            };
            fileServerOptions.StaticFileOptions.ContentTypeProvider = new FileExtensionContentTypeProvider();
            app.UseFileServer(fileServerOptions);

            // Serve the index.html page at /{options.RoutePrefix}/ via middleware
            app.UseMiddleware<SwaggerUiMiddleware>(options);

            return app;
        }
    }
}
