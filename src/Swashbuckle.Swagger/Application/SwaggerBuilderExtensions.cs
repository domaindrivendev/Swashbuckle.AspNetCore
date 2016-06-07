using System;
using Microsoft.AspNetCore.Http;
using Swashbuckle.Swagger.Application;
using Swashbuckle.Swagger.Model;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerBuilderExtensions
    {
        public static IApplicationBuilder UseSwagger(
            this IApplicationBuilder app,
            string routeTemplate = "swagger/{apiVersion}/swagger.json")
        {
            return app.UseSwagger(NullDocumentFilter, routeTemplate);
        }

        public static IApplicationBuilder UseSwagger(
            this IApplicationBuilder app,
            Action<HttpRequest, SwaggerDocument> documentFilter,
            string routeTemplate = "swagger/{apiVersion}/swagger.json")
        {
            return app.UseMiddleware<SwaggerMiddleware>(documentFilter, routeTemplate);
        }

        private static void NullDocumentFilter(HttpRequest httpRequest, SwaggerDocument swaggerDoc)
        {}
    }
}