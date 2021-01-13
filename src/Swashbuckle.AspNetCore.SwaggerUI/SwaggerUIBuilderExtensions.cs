using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerUIBuilderExtensions
    {
        /// <summary>
        /// Register the SwaggerUI middleware. Use this overload for default settings or if you want to inject SwaggerUiOptions options via DI
        /// </summary>
        public static IApplicationBuilder UseSwaggerUI(this IApplicationBuilder app)
        {
            return app.UseSwaggerUI(app.ApplicationServices.GetRequiredService<IOptions<SwaggerUIOptions>>().Value);
        }

        /// <summary>
        /// Register the SwaggerUI middleware with provided options
        /// </summary>
        public static IApplicationBuilder UseSwaggerUI(this IApplicationBuilder app, SwaggerUIOptions options)
        {
            // To simplify the common case, use a default that will work with the SwaggerMiddleware defaults
            if (options.ConfigObject.Urls == null)
            {
                options.ConfigObject.Urls = new[] { new UrlDescriptor { Name = "V1 Docs", Url = "/swagger/v1/swagger.json" } };
            }

            return app.UseMiddleware<SwaggerUIMiddleware>(options);
        }

        /// <summary>
        /// Register the SwaggerUI middleware with provided options
        /// </summary>
        public static IApplicationBuilder UseSwaggerUI(this IApplicationBuilder app, Action<SwaggerUIOptions> setupAction)
        {
            var options = app.ApplicationServices.GetRequiredService<IOptions<SwaggerUIOptions>>().Value;
            setupAction(options);

            return app.UseSwaggerUI(options);
        }
    }
}
