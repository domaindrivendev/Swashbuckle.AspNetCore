using System;
using System.Threading.Tasks;
using Xunit;

namespace Swashbuckle.IntegrationTests
{
    public class SwaggerUiIntegrationTests
    {
        [Fact]
        public async Task CustomStylesheetsAndScriptsCanBeInjected()
        {
            var client = new TestSite(typeof(CustomAssets.Startup)).BuildClient();

            var response = await client.GetAsync("/swagger/ui/index.html");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("<script src='/ext/custom-script.js' type='text/javascript'></script>", content);
            Assert.Contains("<link href='/ext/custom-stylesheet.css' media='screen' rel='stylesheet' type='text/css' />", content);
        }
    }
}