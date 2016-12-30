using System.Threading.Tasks;
using Xunit;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
    public class SwaggerUiIntegrationTests
    {
        [Fact]
        public async Task SwaggerUiRequest_IncludesCustomStylesheetsAndScripts_IfConfigured()
        {
            var client = new TestSite(typeof(CustomUiConfig.Startup)).BuildClient();

            var response = await client.GetAsync("/swagger/index.html");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("/ext/custom-script.js", content);
            Assert.Contains("<link href='/ext/custom-stylesheet.css' rel='stylesheet' media='screen' type='text/css' />", content);
        }
    }
}