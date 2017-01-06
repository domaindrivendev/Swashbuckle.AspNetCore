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

            // Redirect base path to index page
            app.UseMiddleware<RedirectMiddleware>(options.RoutePrefix, options.IndexPath);

            // Serve indexPath via middleware
            app.UseMiddleware<SwaggerUiMiddleware>(options);

            // Serve everything else via static file server
            var fileServerOptions = new FileServerOptions
            {
                RequestPath = $"/{options.RoutePrefix}",
                EnableDefaultFiles = false,
                FileProvider = new EmbeddedFileProvider(typeof(SwaggerUiBuilderExtensions).GetTypeInfo().Assembly,
                    "Swashbuckle.AspNetCore.SwaggerUi.bower_components.swagger_ui.dist")
            };
            fileServerOptions.StaticFileOptions.ContentTypeProvider = new FileExtensionContentTypeProvider();
            app.UseFileServer(fileServerOptions);

            return app;
        }
    }
}
