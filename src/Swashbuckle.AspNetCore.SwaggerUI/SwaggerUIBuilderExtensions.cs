using System;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerUIBuilderExtensions
    {
        private const string NpmEmbeddedFilesNamespace = "Swashbuckle.AspNetCore.SwaggerUI.node_modules.swagger_ui_dist";
        private const string JsEmbeddedFilesNamespace = "Swashbuckle.AspNetCore.SwaggerUI.js";
        private const string CssEmbeddedFilesNamespace = "Swashbuckle.AspNetCore.SwaggerUI.css";

        public static IApplicationBuilder UseSwaggerUI(
            this IApplicationBuilder app,
            Action<SwaggerUIOptions> setupAction = null)
        {
            var options = new SwaggerUIOptions();
            setupAction?.Invoke(options);

            var assembly = typeof(SwaggerUIBuilderExtensions).GetTypeInfo().Assembly;

            app.UseMiddleware<SwaggerUIIndexMiddleware>(options);
            app.UseFileServer(new FileServerOptions
            {
                RequestPath = string.IsNullOrEmpty(options.RoutePrefix) ? string.Empty : $"/{options.RoutePrefix}",
                FileProvider = new EmbeddedFileProvider(typeof(SwaggerUIBuilderExtensions).GetTypeInfo().Assembly, NpmEmbeddedFilesNamespace),
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = string.IsNullOrEmpty(options.RoutePrefix) ? string.Empty : $"/{options.RoutePrefix}",
                FileProvider = new EmbeddedFileProvider(typeof(SwaggerUIBuilderExtensions).GetTypeInfo().Assembly, JsEmbeddedFilesNamespace),
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = string.IsNullOrEmpty(options.RoutePrefix) ? string.Empty : $"/{options.RoutePrefix}",
                FileProvider = new EmbeddedFileProvider(typeof(SwaggerUIBuilderExtensions).GetTypeInfo().Assembly, CssEmbeddedFilesNamespace),
            });

            return app;
        }
    }
}
