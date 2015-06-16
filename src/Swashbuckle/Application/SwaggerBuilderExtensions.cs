using System.Reflection;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.StaticFiles;
using Swashbuckle.Application;

namespace Microsoft.AspNet.Builder
{
    public static class SwaggerBuilderExtensions
    {
        public static void UseSwagger(
            this IApplicationBuilder app,
            string routeTemplate = "swagger/{apiVersion}/swagger.json")
        {
            // ThrowIfRequiredServicesNotRegistered(app.ApplicationServices);

            app.UseMiddleware<SwaggerDocsMiddleware>(routeTemplate);
        }

        public static void  UseSwaggerUi(
            this IApplicationBuilder app,
            string routePrefix = "/swagger/ui",
            string swaggerUrl = "/swagger/v1/swagger.json")
        {
            // Serve index.html via middleware
            app.UseMiddleware<SwaggerUiMiddleware>(routePrefix, swaggerUrl);

            // Serve all other swagger-ui assets as static files
            var options = new FileServerOptions();
            options.RequestPath = routePrefix;
            options.DefaultFilesOptions.DefaultFileNames.Clear();
            options.DefaultFilesOptions.DefaultFileNames.Add("index.html");

            options.StaticFileOptions.ContentTypeProvider = new FileExtensionContentTypeProvider();

            options.FileProvider = new EmbeddedFileProvider(
                typeof(SwaggerBuilderExtensions).GetTypeInfo().Assembly,
                "Swashbuckle.bower_components.swagger_ui.dist");

            app.UseFileServer(options);
        }
    }
}
