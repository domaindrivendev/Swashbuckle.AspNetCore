using Swashbuckle.Swagger.Application;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerBuilderExtensions
    {
        public static IApplicationBuilder UseSwagger(
            this IApplicationBuilder app,
            string routeTemplate = "swagger/{apiVersion}/swagger.json")
        {
            return app.UseMiddleware<SwaggerMiddleware>(routeTemplate);
        }
    }
}