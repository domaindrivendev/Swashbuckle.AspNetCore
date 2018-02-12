using System;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.AspNetCore.ReDoc;

namespace Microsoft.AspNetCore.Builder
{
    public static class ReDocBuilderExtensions
    {
        private const string EmbeddedFilesNamespace = "Swashbuckle.AspNetCore.ReDoc.redoc.dist";

        public static IApplicationBuilder UseReDoc(
            this IApplicationBuilder app,
            Action<ReDocOptions> setupAction)
        {
            var options = new ReDocOptions();
            setupAction?.Invoke(options);

            app.UseMiddleware<ReDocIndexMiddleware>(options);
            app.UseFileServer(new FileServerOptions
            {
                RequestPath = $"/{options.RoutePrefix}",
                FileProvider = new EmbeddedFileProvider(typeof(ReDocBuilderExtensions).GetTypeInfo().Assembly, EmbeddedFilesNamespace),
                EnableDirectoryBrowsing = true // will redirect to /{options.RoutePrefix}/ when trailing slash is missing
            });

            return app;
        }
    }
}
