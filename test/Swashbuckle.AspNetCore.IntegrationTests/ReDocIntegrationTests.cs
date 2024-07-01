using System.Net;
using System.Threading.Tasks;
using Xunit;
using ReDocApp = ReDoc;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
    [Collection("TestSite")]
    public class ReDocIntegrationTests
    {
        [Fact]
        public async Task RoutePrefix_RedirectsToIndexUrl()
        {
            var client = new TestSite(typeof(ReDocApp.Startup)).BuildClient();

            var response = await client.GetAsync("/api-docs");

            Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
            Assert.Equal("api-docs/index.html", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task RedocMiddleware_ReturnsInitializerScript()
        {
            var client = new TestSite(typeof(ReDocApp.Startup)).BuildClient();

            var response = await client.GetAsync("/api-docs/index.js");
            var indexContent = await response.Content.ReadAsStringAsync();

            Assert.Contains("Redoc.init", indexContent);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task IndexUrl_ReturnsEmbeddedVersionOfTheRedocUI()
        {
            var client = new TestSite(typeof(ReDocApp.Startup)).BuildClient();

            var indexResponse = await client.GetAsync("/api-docs/index.html");
            var jsResponse = await client.GetAsync("/api-docs/redoc.standalone.js");

            var indexContent = await indexResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);
        }

        [Fact]
        public async Task IndexUrl_IgnoresUrlCase()
        {
            var client = new TestSite(typeof(ReDocApp.Startup)).BuildClient();

            var indexResponse = await client.GetAsync("/Api-Docs/index.html");
            var jsResponse = await client.GetAsync("/Api-Docs/redoc.standalone.js");

            var indexContent = await indexResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);
        }

        [Theory]
        [InlineData("/redoc/1.0/index.js", "/swagger/1.0/swagger.json")]
        [InlineData("/redoc/2.0/index.js", "/swagger/2.0/swagger.json")]
        public async Task RedocMiddleware_CanBeConfiguredMultipleTimes(string redocUrl, string swaggerPath)
        {
            var client = new TestSite(typeof(MultipleVersions.Startup)).BuildClient();

            var response = await client.GetAsync(redocUrl);
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(swaggerPath, content);
        }
    }
}
