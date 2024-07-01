using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
    [Collection("TestSite")]
    public class SwaggerUIIntegrationTests
    {
        [Theory]
        [InlineData(typeof(Basic.Startup), "/", "index.html")]
        [InlineData(typeof(CustomUIConfig.Startup), "/swagger", "swagger/index.html")]
        [InlineData(typeof(CustomUIConfig.Startup), "/swagger/", "index.html")]
        public async Task RoutePrefix_RedirectsToPathRelativeIndexUrl(
            Type startupType,
            string requestPath,
            string expectedRedirectPath)
        {
            var client = new TestSite(startupType).BuildClient();

            var response = await client.GetAsync(requestPath);

            Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
            Assert.Equal(expectedRedirectPath, response.Headers.Location.ToString());
        }

        [Theory]
        [InlineData(typeof(Basic.Startup), "/index.js")]
        [InlineData(typeof(CustomUIConfig.Startup), "/swagger/index.js")]
        public async Task SwaggerUIMiddleware_ReturnsInitializerScript(
            Type startupType,
            string indexJsPath)
        {
            var client = new TestSite(startupType).BuildClient();

            var indexResponse = await client.GetAsync(indexJsPath);
            Assert.Equal(HttpStatusCode.OK, indexResponse.StatusCode);

            var indexContent = await indexResponse.Content.ReadAsStringAsync();
            Assert.Contains("SwaggerUIBundle", indexContent);
        }

        [Theory]
        [InlineData(typeof(Basic.Startup), "/index.html", "/swagger-ui.js", "/index.css", "/swagger-ui.css")]
        [InlineData(typeof(CustomUIConfig.Startup), "/swagger/index.html", "/swagger/swagger-ui.js", "swagger/index.css", "/swagger/swagger-ui.css")]
        public async Task IndexUrl_ReturnsEmbeddedVersionOfTheSwaggerUI(
            Type startupType,
            string indexPath,
            string swaggerUijsPath,
            string indexCssPath,
            string swaggerUiCssPath)
        {
            var client = new TestSite(startupType).BuildClient();

            var indexResponse = await client.GetAsync(indexPath);
            Assert.Equal(HttpStatusCode.OK, indexResponse.StatusCode);

            var jsResponse = await client.GetAsync(swaggerUijsPath);
            Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);

            var cssResponse = await client.GetAsync(indexCssPath);
            Assert.Equal(HttpStatusCode.OK, cssResponse.StatusCode);

            cssResponse = await client.GetAsync(swaggerUiCssPath);
            Assert.Equal(HttpStatusCode.OK, cssResponse.StatusCode);
        }

        [Fact]
        public async Task IndexUrl_ReturnsCustomPageTitleAndStylesheets_IfConfigured()
        {
            var client = new TestSite(typeof(CustomUIConfig.Startup)).BuildClient();

            var response = await client.GetAsync("/swagger/index.html");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("<title>CustomUIConfig</title>", content);
            Assert.Contains("<link href='/ext/custom-stylesheet.css' rel='stylesheet' media='screen' type='text/css' />", content);
        }

        [Fact]
        public async Task IndexUrl_ReturnsCustomIndexHtml_IfConfigured()
        {
            var client = new TestSite(typeof(CustomUIIndex.Startup)).BuildClient();

            var response = await client.GetAsync("/swagger/index.html");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Example.com", content);
        }

        [Fact]
        public async Task IndexUrl_ReturnsInterceptors_IfConfigured()
        {
            var client = new TestSite(typeof(CustomUIConfig.Startup)).BuildClient();

            var response = await client.GetAsync("/swagger/index.js");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("\"RequestInterceptorFunction\":", content);
            Assert.Contains("\"ResponseInterceptorFunction\":", content);
        }

        [Theory]
        [InlineData("/swagger/index.js", new[] { "Version 1.0", "Version 2.0" })]
        [InlineData("/swagger/1.0/index.js", new[] { "Version 1.0" })]
        [InlineData("/swagger/2.0/index.js", new[] { "Version 2.0" })]
        public async Task SwaggerUIMiddleware_CanBeConfiguredMultipleTimes(string swaggerUiUrl, string[] versions)
        {
            var client = new TestSite(typeof(MultipleVersions.Startup)).BuildClient();

            var response = await client.GetAsync(swaggerUiUrl);
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            foreach (var version in versions)
            {
                Assert.Contains(version, content);
            }
        }

        [Theory]
        [InlineData(typeof(Basic.Startup), "/index.html", "./swagger-ui.css", "./swagger-ui-bundle.js", "./swagger-ui-standalone-preset.js")]
        [InlineData(typeof(CustomUIConfig.Startup), "/swagger/index.html", "/ext/custom-stylesheet.css", "/ext/custom-javascript.js", "/ext/custom-javascript.js")]
        public async Task IndexUrl_Returns_ExpectedAssetPaths(
            Type startupType,
            string indexPath,
            string cssPath,
            string scriptBundlePath,
            string scriptPresetsPath)
        {
            var client = new TestSite(startupType).BuildClient();

            var indexResponse = await client.GetAsync(indexPath);
            Assert.Equal(HttpStatusCode.OK, indexResponse.StatusCode);
            var content = await indexResponse.Content.ReadAsStringAsync();

            Assert.Contains($"<link rel=\"stylesheet\" type=\"text/css\" href=\"{cssPath}\">", content);
            Assert.Contains($"<script src=\"{scriptBundlePath}\">", content);
            Assert.Contains($"<script src=\"{scriptPresetsPath}\">", content);
        }
    }
}
