using System;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

namespace Microsoft.AspNet.Builder
{
    public static class SwaggerBuilderExtensions
    {
        public static void UseSwagger(
            this IApplicationBuilder app,
            string routeTemplate = "swagger/{apiVersion}/swagger.json")
        {
            ThrowIfServiceNotRegistered(app.ApplicationServices);

            app.UseMiddleware<SwaggerDocsMiddleware>(routeTemplate);
        }

        private static void ThrowIfServiceNotRegistered(IServiceProvider applicationServices)
        {
            var service = applicationServices.GetService(typeof(ISwaggerProvider));
            if (service == null)
                throw new InvalidOperationException(string.Format(
                    "Required service 'ISwaggerProvider' not registered - are you missing a call to AddSwagger?"));
        }
    }
}
