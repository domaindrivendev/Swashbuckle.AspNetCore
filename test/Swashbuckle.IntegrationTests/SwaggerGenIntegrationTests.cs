using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Swashbuckle.IntegrationTests
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
        [InlineData(typeof(CustomizedUi.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(MultipleVersions.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(MultipleVersions.Startup), "/swagger/v2/swagger.json")]
        [InlineData(typeof(SecuritySchemes.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(VirtualDirectory.Startup), "/vdir/swagger/v1/swagger.json")]
        public async Task SwaggerRequest_ReturnsValidSwaggerJson(
            Type startupType,
            string swaggerRequestUri)
        {
            var client = new TestServer(TestServer.CreateBuilder()
                .UseStartup(startupType)
                // Use a Convention to only surface ApiDescriptions if action belongs to test app assembly
                .UseServices(services =>
                    services.Configure<MvcOptions>(c => c.Conventions.Add(new TestAppActionModel(startupType.Assembly)))
                ))
                .CreateClient();

            var swaggerResponse = await client.GetAsync(swaggerRequestUri);

            swaggerResponse.EnsureSuccessStatusCode();
            await AssertValidSwaggerAsync(swaggerResponse);
        }

        private async Task AssertValidSwaggerAsync(HttpResponseMessage swaggerResponse)
        {
            var validationResponse = await _validatorClient.PostAsync("/validator/debug", swaggerResponse.Content);

            validationResponse.EnsureSuccessStatusCode();
            var validationErrorsString = await validationResponse.Content.ReadAsStringAsync();
            _output.WriteLine(validationErrorsString);
            Assert.Equal("[]", validationErrorsString);
        }
    }
}