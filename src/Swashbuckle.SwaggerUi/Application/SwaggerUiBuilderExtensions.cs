using System;
using System.Reflection;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.SwaggerUi.Application;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerUiBuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerUi(
            this IApplicationBuilder app,
            Action<SwaggerUiOptions> setupAction = null)
        {
            var options = new SwaggerUiOptions();
            setupAction?.Invoke(options);

            // Enable redirect from basePath to indexPath
            app.UseMiddleware<RedirectMiddleware>(options.BaseRoute, options.IndexPath);

            // Serve indexPath via middleware
            app.UseMiddleware<SwaggerUiMiddleware>(options);

            // Serve everything else via static file server
            var fileServerOptions = new FileServerOptions
            {
                RequestPath = $"/{options.BaseRoute}",
                EnableDefaultFiles = false,
                FileProvider = new EmbeddedFileProvider(typeof(SwaggerUiBuilderExtensions).GetTypeInfo().Assembly,
                    "Swashbuckle.SwaggerUi.bower_components.swagger_ui.dist")
            };
            fileServerOptions.StaticFileOptions.ContentTypeProvider = new FileExtensionContentTypeProvider();
            app.UseFileServer(fileServerOptions);

            return app;
        }
    }
}
