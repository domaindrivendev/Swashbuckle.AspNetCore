using System.Reflection;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.SwaggerUi.Application;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerUiBuilderExtensions
    {
        public static IApplicationBuilder  UseSwaggerUi(
            this IApplicationBuilder app,
            string baseRoute = "swagger/ui",
            string swaggerUrl = "/swagger/v1/swagger.json")
        {
            baseRoute.Trim('/');
            var indexPath = baseRoute + "/index.html";

            // Enable redirect from basePath to indexPath
            app.UseMiddleware<RedirectMiddleware>(baseRoute, indexPath);

            // Serve indexPath via middleware
            app.UseMiddleware<SwaggerUiMiddleware>(indexPath, swaggerUrl);

            // Serve all other swagger-ui assets as static files
            var options = new FileServerOptions();
            options.RequestPath = "/" + baseRoute;
            options.EnableDefaultFiles = false;
            options.StaticFileOptions.ContentTypeProvider = new FileExtensionContentTypeProvider();

            var embedFiles = typeof(SwaggerUiBuilderExtensions).GetTypeInfo().Assembly.GetManifestResourceNames();

            options.FileProvider = new EmbeddedFileProvider(typeof(SwaggerUiBuilderExtensions).GetTypeInfo().Assembly,
                "Swashbuckle.SwaggerUi.bower_components.swagger_ui.dist");

            app.UseFileServer(options);
            return app;
        }
    }
}
