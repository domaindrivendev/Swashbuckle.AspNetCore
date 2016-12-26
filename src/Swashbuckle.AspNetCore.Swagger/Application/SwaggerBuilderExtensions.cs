using System;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Swagger;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerBuilderExtensions
    {
        public static IApplicationBuilder UseSwagger(
            this IApplicationBuilder app,
            string routeTemplate = "swagger/{documentName}/swagger.json",
            Action<SwaggerDocument, HttpRequest> documentFilter = null)
        {
            return app.UseMiddleware<SwaggerMiddleware>(routeTemplate, documentFilter ?? PassThroughDocumentFilter);
        }

        private static void PassThroughDocumentFilter(SwaggerDocument swaggerDoc, HttpRequest httpRequest)
        {}
    }
}