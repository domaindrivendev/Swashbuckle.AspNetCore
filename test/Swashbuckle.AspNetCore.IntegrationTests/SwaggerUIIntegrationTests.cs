using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
    public class SwaggerUIIntegrationTests
    {

        [Fact]
        public async Task SwaggerUIIndex_ServesAnEmbeddedVersionOfTheSwaggerUI()
        {
            var client = new TestSite(typeof(Basic.Startup)).BuildClient();

            var indexResponse = await client.GetAsync("/"); // Basic is configured to serve UI at root
            var jsResponse = await client.GetAsync("/swagger-ui.js");
            var cssResponse = await client.GetAsync("/swagger-ui.css");

            var indexContent = await indexResponse.Content.ReadAsStringAsync();
            Assert.Contains("SwaggerUIBundle", indexContent);
            Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, cssResponse.StatusCode);
        }

        [Fact]
        public async Task SwaggerUIIndex_RedirectsToTrailingSlash_IfNotProvided()
        {
            var client = new TestSite(typeof(CustomUIConfig.Startup)).BuildClient();

            var response = await client.GetAsync("/swagger");

            Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
            Assert.Equal("swagger/", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task SwaggerUIIndex_IncludesCustomPageTitleAndStylesheets_IfConfigured()
        {
            var client = new TestSite(typeof(CustomUIConfig.Startup)).BuildClient();

            var response = await client.GetAsync("/swagger/");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("<title>CustomUIConfig</title>", content);
            Assert.Contains("<link href='/ext/custom-stylesheet.css' rel='stylesheet' media='screen' type='text/css' />", content);
        }

        [Fact]
        public async Task SwaggerUIIndex_ServesCustomIndexHtml_IfConfigured()
        {
            var client = new TestSite(typeof(CustomUIIndex.Startup)).BuildClient();

            var response = await client.GetAsync("/swagger/");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Topbar", content);
        }

    }
}