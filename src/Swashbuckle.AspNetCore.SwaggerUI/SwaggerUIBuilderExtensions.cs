using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerUIBuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerUI(
            this IApplicationBuilder app,
            Action<SwaggerUIOptions> setupAction = null,
            bool ignoreExistingOptions = false)
        {
            SwaggerUIOptions options = new SwaggerUIOptions();

            var existingSwaggerOptions = app.ApplicationServices.GetService<IOptions<SwaggerUIOptions>>();
            if (ignoreExistingOptions == false && existingSwaggerOptions?.Value != null)
            {
                options = existingSwaggerOptions.Value;
            }
            
            setupAction?.Invoke(options);
            app.UseMiddleware<SwaggerUIMiddleware>(options);

            return app;
        }
    }
}
