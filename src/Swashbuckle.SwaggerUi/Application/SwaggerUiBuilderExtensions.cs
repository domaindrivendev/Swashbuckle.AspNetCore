using System;
using System.Reflection;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.SwaggerUi.Application;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerUiBuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerUi(this IApplicationBuilder app, Action<SwaggerUiConfig> configure = null)
        {
            var config = new SwaggerUiConfig();
            configure?.Invoke(config);

            // Enable redirect from basePath to indexPath
            app.UseMiddleware<RedirectMiddleware>(config.BaseRoute, config.IndexPath);

            // Serve indexPath via middleware
            app.UseMiddleware<SwaggerUiMiddleware>(config);

            var options = new FileServerOptions();
            options.RequestPath = $"/{config.BaseRoute}";
            options.EnableDefaultFiles = false;
            options.StaticFileOptions.ContentTypeProvider = new FileExtensionContentTypeProvider();

            options.FileProvider = new EmbeddedFileProvider(typeof(SwaggerUiBuilderExtensions).GetTypeInfo().Assembly,
                "Swashbuckle.SwaggerUi.bower_components.swagger_ui.dist");

            app.UseFileServer(options);

            return app;
        }
    }
}
