using System.Reflection;
using Microsoft.Framework.DependencyInjection;
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

            var swaggerPathHelper = app.ApplicationServices.GetRequiredService<SwaggerPathHelper>();
            swaggerPathHelper.SetRouteTemplate(routeTemplate);

            app.UseMiddleware<SwaggerDocsMiddleware>(routeTemplate);
        }

        public static void  UseSwaggerUi(
            this IApplicationBuilder app,
            string requestPath = "swagger/ui")
        {
            // Serve index.html via middleware
            var indexPath = requestPath.Trim('/') + "/index.html";
            app.UseMiddleware<SwaggerUiMiddleware>(indexPath);

            // Serve all other swagger-ui assets as static files
            var options = new FileServerOptions();
            options.RequestPath = "/" + requestPath;
            options.EnableDefaultFiles = false;
            options.StaticFileOptions.ContentTypeProvider = new FileExtensionContentTypeProvider();
            options.FileProvider = new EmbeddedFileProvider(
                typeof(SwaggerBuilderExtensions).GetTypeInfo().Assembly,
                "Swashbuckle.bower_components.swagger_ui.dist");

            app.UseFileServer(options);
        }
    }
}
