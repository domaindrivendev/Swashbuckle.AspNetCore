using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
    public class SwaggerUIIntegrationTests
    {
        [Fact]
        public async Task RoutePrefix_RedirectsToRelativeIndexUrl()
        {
            var client = new TestSite(typeof(CustomUIConfig.Startup)).BuildClient();

            var response = await client.GetAsync("/swagger");

            Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
            Assert.Equal("swagger/index.html", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task RoutePrefix_RedirectsSubAppRelativeIndexUrl()
        {
            var client = new TestSite(typeof(SubApp.Startup)).BuildClient();
            var response = await client.GetAsync("/subapp/");
            Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
            Assert.Equal("/subapp/index.html", response.Headers.Location.ToString());
        }


        [Fact]
        public async Task IndexUrl_ReturnsEmbeddedVersionOfTheSwaggerUI()
        {
            var client = new TestSite(typeof(Basic.Startup)).BuildClient();

            var indexResponse = await client.GetAsync("/index.html"); // Basic is configured to serve UI at root
            var jsResponse = await client.GetAsync("/swagger-ui.js");
            var cssResponse = await client.GetAsync("/swagger-ui.css");

            var indexContent = await indexResponse.Content.ReadAsStringAsync();
            Assert.Contains("SwaggerUIBundle", indexContent);
            Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);
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
    }
}