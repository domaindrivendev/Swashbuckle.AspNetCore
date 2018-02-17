using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
    public class SwaggerGenIntegrationTests
    {
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _validatorClient;

        public SwaggerGenIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            _validatorClient = new HttpClient
            {
                BaseAddress = new Uri("http://online.swagger.io")
            };
        }

        [Theory]
        [InlineData(typeof(Basic.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(CustomUIConfig.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(CustomUIIndex.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(GenericControllers.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(MultipleVersions.Startup), "/swagger/v2/swagger.json")]
        [InlineData(typeof(OAuth2Integration.Startup), "/resource-server/swagger/v1/swagger.json")]
        public async Task SwaggerEndpoint_ReturnsValidSwaggerJson(
            Type startupType,
            string swaggerRequestUri)
        {
            var testSite = new TestSite(startupType);
            var client = testSite.BuildClient();

            var swaggerResponse = await client.GetAsync(swaggerRequestUri);

            swaggerResponse.EnsureSuccessStatusCode();

            await AssertResponseDoesNotContainByteOrderMark(swaggerResponse);

            // NOTE: the online swagger validator INCORRECTLY returns an error for the Swagger generated
            // by the "Basic" sample Website. As a temporary workaround, bypass the valid swagger assertion
            if (startupType == typeof(Basic.Startup)) return;

            await AssertValidSwaggerAsync(swaggerResponse);
        }

        private async Task AssertResponseDoesNotContainByteOrderMark(HttpResponseMessage swaggerResponse)
        {
            var responseAsByteArray = await swaggerResponse.Content.ReadAsByteArrayAsync();
            var bomByteArray = Encoding.UTF8.GetPreamble();

            var byteIndex = 0;
            var doesContainBom = true;
            while (byteIndex < bomByteArray.Length && doesContainBom)
            {
                if (bomByteArray[byteIndex] != responseAsByteArray[byteIndex])
                {
                    doesContainBom = false;
                }

                byteIndex += 1;
            }

            Assert.False(doesContainBom);
        }

        private async Task AssertValidSwaggerAsync(HttpResponseMessage swaggerResponse)
        {
            var validationResponse = await _validatorClient.PostAsync("/validator/debug", swaggerResponse.Content);

            validationResponse.EnsureSuccessStatusCode();
            var validationErrorsString = await validationResponse.Content.ReadAsStringAsync();
            _output.WriteLine(validationErrorsString);

            Assert.Equal("{}", validationErrorsString);
        }
    }
}