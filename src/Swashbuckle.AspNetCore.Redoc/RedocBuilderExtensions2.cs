using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Redoc;

namespace Microsoft.AspNetCore.Builder
{
    public static class RedocBuilderExtensions
    {
        /// <summary>
        /// Register the Redoc middleware with provided options
        /// </summary>
        public static IApplicationBuilder UseRedoc(this IApplicationBuilder app, RedocOptions options)
        {
            return app.UseMiddleware<RedocMiddleware>(options);
        }

        /// <summary>
        /// Register the Redoc middleware with optional setup action for DI-injected options
        /// </summary>
        public static IApplicationBuilder UseRedoc(
            this IApplicationBuilder app,
            Action<RedocOptions> setupAction = null)
        {
            RedocOptions options;
            using (var scope = app.ApplicationServices.CreateScope())
            {
                options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<RedocOptions>>().Value;
                setupAction?.Invoke(options);
            }

            // To simplify the common case, use a default that will work with the SwaggerMiddleware defaults
            if (options.SpecUrl == null)
            {
                options.SpecUrl = "../swagger/v1/swagger.json";
            }

            return app.UseRedoc(options);
        }
    }
}
