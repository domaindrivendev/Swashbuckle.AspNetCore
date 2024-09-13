using System.Net;
using System.Threading.Tasks;
using Xunit;
using ScalarApp = Scalar;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
    [Collection("TestSite")]
    public class ScalarIntegrationTests
    {
        [Fact]
        public async Task HtmlContainsExpectedContent()
        {
            var client = new TestSite(typeof(ScalarApp.Startup)).BuildClient();

            var response = await client.GetAsync("/api-reference/index.html");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("theme", content);
            Assert.Contains("moon", content);
            Assert.Contains("standalone.js", content);
        }

        [Fact]
        public async Task RoutePrefix_RedirectsToIndexUrl()
        {
            var client = new TestSite(typeof(ScalarApp.Startup)).BuildClient();

            var response = await client.GetAsync("/api-reference");

            Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
            Assert.Equal("api-reference/index.html", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task IndexUrl_ReturnsEmbeddedVersionOfTheRedocUI()
        {
            var client = new TestSite(typeof(ScalarApp.Startup)).BuildClient();

            var htmlResponse = await client.GetAsync("/api-reference/index.html");
            var jsResponse = await client.GetAsync("/api-reference/standalone.js");

            Assert.Equal(HttpStatusCode.OK, htmlResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);
        }

        [Fact]
        public async Task IndexUrl_IgnoresUrlCase()
        {
            var client = new TestSite(typeof(ScalarApp.Startup)).BuildClient();

            var htmlResponse = await client.GetAsync("/Api-Reference/index.html");
            var jsRedocResponse = await client.GetAsync("/Api-Reference/standalone.js");

            Assert.Equal(HttpStatusCode.OK, htmlResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, jsRedocResponse.StatusCode);
        }

        [Theory]
        [InlineData("/scalar/1.0/index.html", "/swagger/1.0/swagger.json")]
        [InlineData("/scalar/2.0/index.html", "/swagger/2.0/swagger.json")]
        public async Task RedocMiddleware_CanBeConfiguredMultipleTimes(string htmlUrl, string swaggerPath)
        {
            var client = new TestSite(typeof(MultipleVersions.Startup)).BuildClient();

            var htmlResponse = await client.GetAsync(htmlUrl);
            var content = await htmlResponse.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, htmlResponse.StatusCode);
            Assert.Contains(swaggerPath, content);
        }
    }
}
