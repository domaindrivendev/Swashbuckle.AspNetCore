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
            Action<SwaggerOptions> setupAction = null,
            bool ignoreExistingOptions = false)
        {
            SwaggerOptions options = new SwaggerOptions();

            var existingSwaggerOptions = app.ApplicationServices.GetService<IOptions<SwaggerOptions>>();
            if (ignoreExistingOptions == false && existingSwaggerOptions?.Value != null)
            {
                options = existingSwaggerOptions.Value;
            }

            setupAction?.Invoke(options);
            app.UseMiddleware<SwaggerMiddleware>(options);

            return app;
        }
    }
}