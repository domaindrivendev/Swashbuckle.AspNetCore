using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public static class SwaggerRouteBuilderExtensions
    {
        public static void EnableSwagger(
            this IRouteBuilder routeBuilder,
            string routeTemplate = "swagger/{apiVersion}/swagger.json")
        {
            ThrowIfSwaggerServicesNotRegistered(routeBuilder.ServiceProvider);

            routeBuilder.MapRoute(
                "swagger_docs",
                routeTemplate,
                new { controller = "SwaggerDocs", action = "GetDocs" });
        }

        private static void ThrowIfSwaggerServicesNotRegistered(IServiceProvider serviceProvider)
        {
            var rootUrlResolver = serviceProvider.GetService(typeof(Func<HttpRequest, string>));
            var swaggerProvider = serviceProvider.GetService(typeof(ISwaggerProvider));

            if (rootUrlResolver == null || swaggerProvider == null)
                throw new InvalidOperationException(
                    "Unable to find required services. To use the Swagger endpoint, you first need to add the " +
                    "services by placing a call to services.AddSwagger within the ConfigureServices method");
        }
    }
}