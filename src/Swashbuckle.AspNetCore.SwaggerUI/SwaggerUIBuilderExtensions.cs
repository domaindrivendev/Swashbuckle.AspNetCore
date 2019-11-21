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
            Action<SwaggerUIOptions> setupAction = null)
        {
            var options = app.ApplicationServices.GetService<IOptions<SwaggerUIOptions>>()?.Value ?? new SwaggerUIOptions();
            setupAction?.Invoke(options);
            app.UseMiddleware<SwaggerUIMiddleware>(options);

            return app;
        }
    }
}
