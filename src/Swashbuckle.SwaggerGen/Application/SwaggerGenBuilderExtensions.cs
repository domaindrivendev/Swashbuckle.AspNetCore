using System;
using Swashbuckle.SwaggerGen.Application;
using Swashbuckle.SwaggerGen.Generator;

namespace Microsoft.AspNet.Builder
{
    public static class SwaggerGenBuilderExtensions
    {
        public static void UseSwaggerGen(
            this IApplicationBuilder app,
            string routeTemplate = "swagger/{apiVersion}/swagger.json")
        {
            ThrowIfServiceNotRegistered(app.ApplicationServices);

            app.UseMiddleware<SwaggerGenMiddleware>(routeTemplate);
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
