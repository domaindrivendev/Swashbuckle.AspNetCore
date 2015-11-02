using System;
using System.Reflection;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.StaticFiles;
using Swashbuckle.Application;

namespace Microsoft.AspNet.Builder
{
    public static class SwaggerUiBuilderExtensions
    {
        public static void  UseSwaggerUi(
            this IApplicationBuilder app,
            string basePath = "swagger/ui")
        {
            ThrowIfServicesNotRegistered(app.ApplicationServices);

            basePath = basePath.Trim('/');
            var indexPath = basePath + "/index.html";

            // Enable redirect from basePath to indexPath
            app.UseMiddleware<RedirectMiddleware>(basePath, indexPath);

            // Serve indexPath via middleware
            app.UseMiddleware<SwaggerUiMiddleware>(indexPath);

            // Serve all other swagger-ui assets as static files
            var options = new FileServerOptions();
            options.RequestPath = "/" + basePath;
            options.EnableDefaultFiles = false;
            options.StaticFileOptions.ContentTypeProvider = new FileExtensionContentTypeProvider();
            options.FileProvider = new EmbeddedFileProvider(
                typeof(SwaggerUiBuilderExtensions).GetTypeInfo().Assembly,
                "Swashbuckle.bower_components.swagger_ui.dist");

            app.UseFileServer(options);
        }

        private static void ThrowIfServicesNotRegistered(IServiceProvider applicationServices)
        {
            var requiredServices = new[] { typeof(SwaggerPathHelper) };
            foreach (var type in requiredServices)
            {
                var service = applicationServices.GetService(type);
                if (service == null)
                    throw new InvalidOperationException(string.Format(
                        "Required service '{0}' not registered - are you missing a call to AddSwagger?", type));
            }
        }
    }
}
