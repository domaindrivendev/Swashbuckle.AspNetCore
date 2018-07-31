using System;
using Swashbuckle.AspNetCore.ReDoc;

namespace Microsoft.AspNetCore.Builder
{
    public static class ReDocBuilderExtensions
    {
        private const string EmbeddedFilesNamespace = "Swashbuckle.AspNetCore.ReDoc.node_modules.redoc.dist";

        public static IApplicationBuilder UseReDoc(
            this IApplicationBuilder app,
            Action<ReDocOptions> setupAction = null)
        {
            if (setupAction == null)
            {
                // Don't pass options so it can be configured/injected via DI container instead
                app.UseMiddleware<ReDocMiddleware>();
            }
            else
            {
                // Configure an options instance here and pass directly to the middleware
                var options = new ReDocOptions();
                setupAction.Invoke(options);

                app.UseMiddleware<ReDocMiddleware>(options);
            }

            return app;
        }
    }
}
