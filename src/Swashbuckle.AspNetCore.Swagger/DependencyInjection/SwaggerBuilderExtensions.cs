using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerBuilderExtensions
    {
        public static IApplicationBuilder UseSwagger(
            this IApplicationBuilder app,
            Action<SwaggerOptions> setupAction = null)
        {
            var options = app.ApplicationServices.GetService<IOptions<SwaggerOptions>>()?.Value ?? new SwaggerOptions();
            setupAction?.Invoke(options);
            app.UseMiddleware<SwaggerMiddleware>(options);

            return app;
        }
    }
}