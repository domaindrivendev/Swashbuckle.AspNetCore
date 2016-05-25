using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Swashbuckle.IntegrationTests
{
    public class SwaggerUiIntegrationTests
    {
        [Theory]
        [InlineData("index.html")]
        [InlineData("css/style.css")]
        [InlineData("swagger-ui.js")]
        public void SwaggerUiAssembly_ContainsEmbeddedFiles(string filePath)
        {
            var fileProvider = new EmbeddedFileProvider(
                typeof(SwaggerUiBuilderExtensions).GetTypeInfo().Assembly,
                "Swashbuckle.SwaggerUi.bower_components.swagger_ui.dist"
            );

            var fileInfo = fileProvider.GetFileInfo(filePath);
            var directoryInfo = fileProvider.GetDirectoryContents("/");

            Assert.True(fileInfo.Exists, $"File '{filePath}' doesn't exist.");
        }
    }
}
