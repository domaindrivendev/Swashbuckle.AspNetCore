using System;
using System.Reflection;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.FileProviders;
using Swashbuckle.Swagger;
using Swashbuckle.Application;

namespace Microsoft.AspNet.Builder
{
    public static class BuilderExtensions
    {
        public static IApplicationBuilder UseSwagger(
            this IApplicationBuilder app,
            string routeTemplate = "swagger/{apiVersion}/swagger.json")
        {
            var routeBuilder = new RouteBuilder
            {
                DefaultHandler = CreateSwaggerDocsHandler(app.ApplicationServices),
                ServiceProvider = app.ApplicationServices
            };

            routeBuilder.MapRoute("swagger_docs", routeTemplate);

            return app.UseRouter(routeBuilder.Build());
        }

        public static IApplicationBuilder UseSwaggerUi(
            this IApplicationBuilder app,
            string routePrefix = "swagger/ui")
        {
            var routeBuilder = new RouteBuilder
            {
                DefaultHandler = CreateSwaggerUiHandler(app.ApplicationServices),
                ServiceProvider = app.ApplicationServices
            };

            var routeTemplate = routePrefix.TrimEnd('/') + "/{*assetPath}";
            routeBuilder.MapRoute("swagger_ui", routeTemplate);

            return app.UseRouter(routeBuilder.Build());
        }

        private static IRouter CreateSwaggerDocsHandler(IServiceProvider serviceProvider)
        {
            var rootUrlResolver = serviceProvider.GetService<IRootUrlResolver>() ?? new DefaultRootUrlResolver();
            var swaggerProvider = serviceProvider.GetRequiredService<ISwaggerProvider>();
            return new SwaggerDocsHandler(rootUrlResolver, swaggerProvider);
        }

        private static IRouter CreateSwaggerUiHandler(IServiceProvider serviceProvider)
        {
            var rootUrlResolver = serviceProvider.GetService<IRootUrlResolver>() ?? new DefaultRootUrlResolver();
            var thisAssembly = typeof(SwaggerUiHandler).GetTypeInfo().Assembly;
            var fileProvider = new EmbeddedFileProvider(thisAssembly, "SwaggerUi");
            return new SwaggerUiHandler(rootUrlResolver, fileProvider);
        }
    }
}
