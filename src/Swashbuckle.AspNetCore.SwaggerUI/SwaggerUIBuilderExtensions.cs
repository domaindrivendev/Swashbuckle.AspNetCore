using System;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerUIBuilderExtensions
    {
        private const string EmbeddedFilesNamespace = "Swashbuckle.AspNetCore.SwaggerUI3.bower_components.swagger_ui.dist";

        public static IApplicationBuilder UseSwaggerUI3(
            this IApplicationBuilder app,
            Action<SwaggerUIOptions> setupAction)
        {
            var options = new SwaggerUIOptions();
            setupAction?.Invoke(options);

            app.UseMiddleware<SwaggerUIIndexMiddleware>(options);
            app.UseFileServer(new FileServerOptions
            {
                RequestPath = string.IsNullOrEmpty(options.RoutePrefix) ? string.Empty : $"/{options.RoutePrefix}",
                FileProvider = new EmbeddedFileProvider(typeof(SwaggerUIBuilderExtensions).GetTypeInfo().Assembly, EmbeddedFilesNamespace),
                EnableDirectoryBrowsing = true // will redirect to /{options.RoutePrefix}/ when trailing slash is missing
            });

            return app;
        }
    }
}
