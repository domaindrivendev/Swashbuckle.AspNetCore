using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
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
        [InlineData(typeof(Basic.Startup), "/index.html", "/swagger-ui.js", "/swagger-ui.css")]
        [InlineData(typeof(CustomUIConfig.Startup), "/swagger/index.html", "/swagger/swagger-ui.js", "/swagger/swagger-ui.css")]
        public async Task IndexUrl_ReturnsEmbeddedVersionOfTheSwaggerUI(
            Type startupType,
            string indexPath,
            string jsPath,
            string cssPath)
        {
            var client = new TestSite(startupType).BuildClient();

            var indexResponse = await client.GetAsync(indexPath);
            Assert.Equal(HttpStatusCode.OK, indexResponse.StatusCode);
            var indexContent = await indexResponse.Content.ReadAsStringAsync();
            Assert.Contains("SwaggerUIBundle", indexContent);

            var jsResponse = await client.GetAsync(jsPath);
            Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);

            var cssResponse = await client.GetAsync(cssPath);
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

        [Theory]
        [InlineData("/swagger/index.html", new [] { "Version 1.0", "Version 2.0" })]
        [InlineData("/swagger/1.0/index.html", new [] { "Version 1.0" })]
        [InlineData("/swagger/2.0/index.html", new [] { "Version 2.0" })]
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
    }
}