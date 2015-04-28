using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Routing;

namespace Swashbuckle.Application
{
    public static class ApplicationBuilderExtensions
    {
        public static void EnableSwaggerUi(
            this IRouteBuilder routeBuilder,
            string routeTemplate = "swagger/ui/{*assetPath}")
        {
            routeBuilder.MapRoute(
                "swagger_ui",
                routeTemplate,
                new { controller = "SwaggerUi", action = "GetAsset" });
        }
    }
}