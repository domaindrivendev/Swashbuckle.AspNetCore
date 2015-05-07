using System;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Http;
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

        private static IRouter CreateSwaggerDocsHandler(IServiceProvider serviceProvider)
        {
            var routeUrlResolver = serviceProvider.GetRequiredService<Func<HttpRequest, string>>();
            var swaggerProvider = serviceProvider.GetRequiredService<ISwaggerProvider>();
            return new SwaggerDocsHandler(routeUrlResolver, swaggerProvider);
        }
    }
}
