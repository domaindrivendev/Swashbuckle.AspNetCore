using System;
using Microsoft.AspNetCore.StaticFiles;
using Swashbuckle.AspNetCore.SwaggerUi;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerUiBuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerUi(
            this IApplicationBuilder app,
            Action<SwaggerUiOptions> setupAction)
        {
            var options = new SwaggerUiOptions();
            setupAction?.Invoke(options);

            // Serve swagger-ui assets with the FileServer middleware, using a custom FileProvider
            // to inject parameters into "index.html"
            var fileServerOptions = new FileServerOptions
            {
                RequestPath = $"/{options.RoutePrefix}",
                FileProvider = new SwaggerUiFileProvider(options.IndexConfig.ToParamDictionary()),
                EnableDefaultFiles = true, // serve index.html at /{options.RoutePrefix}/
            };
            fileServerOptions.StaticFileOptions.ContentTypeProvider = new FileExtensionContentTypeProvider();
            app.UseFileServer(fileServerOptions);

            return app;
        }
    }
}
