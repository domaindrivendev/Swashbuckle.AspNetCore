using System;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.StaticFiles;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

namespace Microsoft.AspNet.Builder
{
    public static class SwaggerBuilderExtensions
    {
        public static void UseSwagger(
            this IApplicationBuilder app,
            string routeTemplate = "swagger/{apiVersion}/swagger.json")
        {
            ThrowIfServicesNotRegistered(app.ApplicationServices,
                new[] { typeof(ISwaggerProvider), typeof(SwaggerPathHelper) });

            app.UseMiddleware<SwaggerDocsMiddleware>(routeTemplate);

            var swaggerPathHelper = app.ApplicationServices.GetRequiredService<SwaggerPathHelper>();
            swaggerPathHelper.SetRouteTemplate(routeTemplate);
        }

        public static void  UseSwaggerUi(
            this IApplicationBuilder app,
            string basePath = "swagger/ui")
        {
            ThrowIfServicesNotRegistered(app.ApplicationServices, new[] { typeof(SwaggerPathHelper) });

            basePath = basePath.Trim('/');
            var indexPath = basePath + "/index.html";

            // Serve index.html via middleware
            app.UseMiddleware<SwaggerUiMiddleware>(indexPath);

            // Serve all other swagger-ui assets as static files
            var options = new FileServerOptions();
            options.RequestPath = "/" + basePath;
            options.EnableDefaultFiles = false;
            options.StaticFileOptions.ContentTypeProvider = new FileExtensionContentTypeProvider();
            options.FileProvider = new EmbeddedFileProvider(
                typeof(SwaggerBuilderExtensions).GetTypeInfo().Assembly,
                "Swashbuckle.bower_components.swagger_ui.dist");

            app.UseFileServer(options);

            // Enable redirect from basePath to index.html
            app.UseMiddleware<RedirectMiddleware>(basePath, indexPath);
        }

        private static void ThrowIfServicesNotRegistered(
            IServiceProvider applicationServices,
            IEnumerable<Type> serviceTypes)
        {
            foreach (var type in serviceTypes)
            {
                var service = applicationServices.GetService(type);
                if (service == null)
                    throw new InvalidOperationException(string.Format(
                        "Required service '{0}' not registered - are you missing a call to AddSwagger?", type));
            }
        }
    }
}
