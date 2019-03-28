using System;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerUIBuilderExtensions
    {
        private const string RootNameSpace = "Swashbuckle.AspNetCore.SwaggerUI";
        private const string NpmEmbeddedFilesNamespace = RootNameSpace + ".node_modules.swagger_ui_dist";
        private const string JsEmbeddedFilesNamespace = RootNameSpace + ".js";
        private const string CssEmbeddedFilesNamespace = RootNameSpace + ".css";

        public static IApplicationBuilder UseSwaggerUI(
            this IApplicationBuilder app,
            Action<SwaggerUIOptions> setupAction = null)
        {
            var options = new SwaggerUIOptions();
            setupAction?.Invoke(options);

            var assembly = typeof(SwaggerUIBuilderExtensions).GetTypeInfo().Assembly;
            var requestPath = string.IsNullOrEmpty(options.RoutePrefix) ? string.Empty : $"/{options.RoutePrefix}";

            app.UseMiddleware<SwaggerUIIndexMiddleware>(options);
            app.UseFileServer(new FileServerOptions
            {
                RequestPath = requestPath,
                FileProvider = new EmbeddedFileProvider(assembly, NpmEmbeddedFilesNamespace),
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = requestPath,
                FileProvider = new EmbeddedFileProvider(assembly, JsEmbeddedFilesNamespace),
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = requestPath,
                FileProvider = new EmbeddedFileProvider(assembly, CssEmbeddedFilesNamespace),
            });

            return app;
        }
    }
}