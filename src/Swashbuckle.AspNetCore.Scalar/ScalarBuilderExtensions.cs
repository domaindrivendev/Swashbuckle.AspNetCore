using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Swashbuckle.AspNetCore.Scalar
{
    public static class ScalarBuilderExtensions
    {
        /// <summary>
        /// Register the Scalar middleware with provided options
        /// </summary>
        public static IApplicationBuilder UseScalar(this IApplicationBuilder app, ScalarOptions options)
        {
            return app.UseMiddleware<ScalarMiddleware>(options);
        }

        /// <summary>
        /// Register the Scalar middleware with optional setup action for DI-injected options
        /// </summary>
        public static IApplicationBuilder UseScalar(
            this IApplicationBuilder app,
            Action<ScalarOptions> setupAction = null)
        {
            ScalarOptions options;
            using (var scope = app.ApplicationServices.CreateScope())
            {
                options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ScalarOptions>>().Value;
                setupAction?.Invoke(options);
            }

            // To simplify the common case, use a default that will work with the SwaggerMiddleware defaults
            if (options.SpecUrl == null)
            {
                options.SpecUrl = "../swagger/v1/swagger.json";
            }

            return app.UseScalar(options);
        }
    }
}
