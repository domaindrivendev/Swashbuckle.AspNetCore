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
        public async Task IndexUrl_ReturnsEmbeddedVersionOfTheRedocUI()
        {
            var client = new TestSite(typeof(ReDocApp.Startup)).BuildClient();

            var htmlResponse = await client.GetAsync("/api-docs/index.html");
            var cssResponse = await client.GetAsync("/api-docs/index.css");
            var jsResponse = await client.GetAsync("/api-docs/redoc.standalone.js");

            Assert.Equal(HttpStatusCode.OK, htmlResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, cssResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);
        }

        [Fact]
        public async Task RedocMiddleware_ReturnsInitializerScript()
        {
            var client = new TestSite(typeof(ReDocApp.Startup)).BuildClient();

            var response = await client.GetAsync("/api-docs/index.js");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Redoc.init", content);
            Assert.DoesNotContain("%(DocumentTitle)", content);
            Assert.DoesNotContain("%(HeadContent)", content);
            Assert.DoesNotContain("%(SpecUrl)", content);
            Assert.DoesNotContain("%(ConfigObject)", content);
        }

        [Fact]
        public async Task IndexUrl_IgnoresUrlCase()
        {
            var client = new TestSite(typeof(ReDocApp.Startup)).BuildClient();

            var htmlResponse = await client.GetAsync("/Api-Docs/index.html");
            var cssResponse = await client.GetAsync("/Api-Docs/index.css");
            var jsInitResponse = await client.GetAsync("/Api-Docs/index.js");
            var jsRedocResponse = await client.GetAsync("/Api-Docs/redoc.standalone.js");

            Assert.Equal(HttpStatusCode.OK, htmlResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, cssResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, jsInitResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, jsRedocResponse.StatusCode);
        }

        [Theory]
        [InlineData("/redoc/1.0/index.html", "/redoc/1.0/index.js", "/swagger/1.0/swagger.json")]
        [InlineData("/redoc/2.0/index.html", "/redoc/2.0/index.js", "/swagger/2.0/swagger.json")]
        public async Task RedocMiddleware_CanBeConfiguredMultipleTimes(string htmlUrl, string jsUrl, string swaggerPath)
        {
            var client = new TestSite(typeof(MultipleVersions.Startup)).BuildClient();

            var htmlResponse = await client.GetAsync(htmlUrl);
            var jsResponse = await client.GetAsync(jsUrl);
            var content = await jsResponse.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, htmlResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);
            Assert.Contains(swaggerPath, content);
        }
    }
}
