using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.ReDoc;

namespace Microsoft.AspNetCore.Builder
{
    public static class ReDocBuilderExtensions
    {
        public static IApplicationBuilder UseReDoc(
            this IApplicationBuilder app,
            Action<ReDocOptions> setupAction = null)
        {
            var options = app.ApplicationServices.GetService<IOptions<ReDocOptions>>()?.Value ?? new ReDocOptions();
            setupAction?.Invoke(options);
            app.UseMiddleware<ReDocMiddleware>(options);

            return app;
        }
    }
}
