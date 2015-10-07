using System;
using Microsoft.Framework.DependencyInjection;
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
            ThrowIfServicesNotRegistered(app.ApplicationServices);

            app.UseMiddleware<SwaggerDocsMiddleware>(routeTemplate);

            var swaggerPathHelper = app.ApplicationServices.GetRequiredService<SwaggerPathHelper>();
            swaggerPathHelper.SetRouteTemplate(routeTemplate);
        }

        private static void ThrowIfServicesNotRegistered(IServiceProvider applicationServices)
        {
            var requiredServices = new[] { typeof(ISwaggerProvider), typeof(SwaggerPathHelper) };
            foreach (var type in requiredServices)
            {
                var service = applicationServices.GetService(type);
                if (service == null)
                    throw new InvalidOperationException(string.Format(
                        "Required service '{0}' not registered - are you missing a call to AddSwagger?", type));
            }
        }
    }
}
