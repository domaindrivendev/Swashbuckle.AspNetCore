using System;
using System.Reflection;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Routing;
using Swashbuckle.Application;

namespace Microsoft.AspNet.Builder
{
    public static class BuilderExtensions
    {
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

        private static IRouter CreateSwaggerUiHandler(IServiceProvider serviceProvider)
        {
            var thisAssembly = typeof(SwaggerUiRouteHandler).GetTypeInfo().Assembly;
            var fileProvider = new EmbeddedFileProvider(thisAssembly, "SwaggerUi");
            return new SwaggerUiRouteHandler(fileProvider);
        }
    }
}
