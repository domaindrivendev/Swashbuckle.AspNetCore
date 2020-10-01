using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.ReDoc;

namespace Microsoft.AspNetCore.Builder
{
    public static class ReDocBuilderExtensions
    {
        /// <summary>
        /// Register the ReDoc middleware. Use this overload for default settings or if you want to inject ReDocOptions options via DI
        /// </summary>
        public static IApplicationBuilder UseReDoc(this IApplicationBuilder app)
        {
            return app.UseReDoc(app.ApplicationServices.GetRequiredService<IOptions<ReDocOptions>>().Value);
        }

        /// <summary>
        /// Register the ReDoc middleware with provided options
        /// </summary>
        public static IApplicationBuilder UseReDoc(this IApplicationBuilder app, ReDocOptions options)
        {
            // To simplify the common case, use a default that will work with the SwaggerMiddleware defaults
            if (options.SpecUrl == null)
            {
                options.SpecUrl = "/swagger/v1/swagger.json";
            }

            return app.UseMiddleware<ReDocMiddleware>(options);
        }

        /// <summary>
        /// Register the ReDoc middleware with provided options
        /// </summary>
        public static IApplicationBuilder UseReDoc(this IApplicationBuilder app, Action<ReDocOptions> setupAction)
        {
            var options = new ReDocOptions();
            setupAction(options);

            return app.UseReDoc(options);
        }
    }
}
