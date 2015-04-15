using Microsoft.AspNet.Builder;

namespace Swashbuckle.Application
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseSwaggerUi(
            this IApplicationBuilder app,
            string routeTemplate = "swagger/ui/{*assetPath}")
        {
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "swagger_ui",
                    routeTemplate,
                    new { controller = "SwaggerUi", action = "GetAsset" });
            });
        }
    }
}