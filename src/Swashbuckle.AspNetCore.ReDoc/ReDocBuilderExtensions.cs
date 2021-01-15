using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.ReDoc;

namespace Microsoft.AspNetCore.Builder
{
    public static class ReDocBuilderExtensions
    {
        /// <summary>
        /// Register the ReDoc middleware with provided options
        /// </summary>
        public static IApplicationBuilder UseReDoc(this IApplicationBuilder app, ReDocOptions options)
        {
            return app.UseMiddleware<ReDocMiddleware>(options);
        }

        /// <summary>
        /// Register the ReDoc middleware with or without additional config action
        /// </summary>
        public static IApplicationBuilder UseReDoc(
            this IApplicationBuilder app,
            Action<ReDocOptions> setupAction = null)
        {
            var options = app.ApplicationServices.GetRequiredService<IOptions<ReDocOptions>>().Value;

            setupAction?.Invoke(options);

            // To simplify the common case, use a default that will work with the SwaggerMiddleware defaults
            if (options.SpecUrl == null)
            {
                options.SpecUrl = "/swagger/v1/swagger.json";
            }

            return app.UseReDoc(options);
        }
    }
}
