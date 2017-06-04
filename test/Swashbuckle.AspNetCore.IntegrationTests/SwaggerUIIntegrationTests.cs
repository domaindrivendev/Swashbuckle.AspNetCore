using System.Threading.Tasks;
using Xunit;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
    public class SwaggerUIIntegrationTests
    {
        [Fact]
        public async Task SwaggerUIIndex_IncludesCustomStylesheetsAndScripts_IfConfigured()
        {
            var client = new TestSite(typeof(CustomUIConfig.Startup)).BuildClient();

            var response = await client.GetAsync("/swagger/");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("/ext/custom-script.js", content);
            Assert.Contains("<link href='/ext/custom-stylesheet.css' rel='stylesheet' media='screen' type='text/css' />", content);
        }

        [Fact]
        public async Task SwaggerUIIndex_PageTitleChanged_IfConfigured()
        {
            var client = new TestSite(typeof(CustomUIConfig.Startup)).BuildClient();

            var response = await client.GetAsync("/swagger/");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("<title>Custom API - Swagger UI</title>", content);
        }
    }
}