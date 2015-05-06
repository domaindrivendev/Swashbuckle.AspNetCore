using System;
using System.Reflection;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Routing;
using Swashbuckle.Application;

namespace Microsoft.AspNet.Builder
{
    public static class BuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerUi(this IApplicationBuilder app)
        {
            var routeBuilder = new RouteBuilder
            {
                DefaultHandler = CreateSwaggerUiHandler(app.ApplicationServices),
                ServiceProvider = app.ApplicationServices
            };

            routeBuilder.MapRoute("swagger_ui", "swagger/ui/{*assetPath}");

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
