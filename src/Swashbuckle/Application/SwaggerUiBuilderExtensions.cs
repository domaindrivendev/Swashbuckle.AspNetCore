using System.Reflection;
using Microsoft.AspNet.StaticFiles;
using Microsoft.AspNet.FileProviders;

namespace Microsoft.AspNet.Builder
{
    public static class SwaggerUiBuilderExtensions
    {
        public static void UseSwaggerUi(
            this IApplicationBuilder app,
            string requestPath = "/swagger/ui")
        {
            var options = new FileServerOptions();

            options.RequestPath = requestPath;
            options.DefaultFilesOptions.DefaultFileNames.Clear();
            options.DefaultFilesOptions.DefaultFileNames.Add("index.html");

            options.StaticFileOptions.ContentTypeProvider = new FileExtensionContentTypeProvider();

            options.FileProvider = new EmbeddedFileProvider(
                typeof(SwaggerUiBuilderExtensions).GetTypeInfo().Assembly,
                "Swashbuckle.bower_components.swagger_ui.dist");

            app.UseFileServer(options);
        }
    }
}
