using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using ReDocApp = ReDoc;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
    [Collection("TestSite")]
    public class SwaggerVerifyIntegrationTest
    {
        [Theory]
        [InlineData(typeof(CliExample.Startup), "/swagger/v1/swagger_net8.0.json")]
        [InlineData(typeof(ConfigFromFile.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(CustomUIConfig.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(CustomUIIndex.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(GenericControllers.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(MultipleVersions.Startup), "/swagger/1.0/swagger.json")]
        [InlineData(typeof(MultipleVersions.Startup), "/swagger/2.0/swagger.json")]
        [InlineData(typeof(OAuth2Integration.Startup), "/resource-server/swagger/v1/swagger.json")]
        [InlineData(typeof(ReDocApp.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(TestFirst.Startup), "/swagger/v1-generated/openapi.json")]
        public async Task SwaggerEndpoint_ReturnsValidSwaggerJson(
            Type startupType,
            string swaggerRequestUri)
        {
            var testSite = new TestSite(startupType);
            using var client = testSite.BuildClient();

            using var swaggerResponse = await client.GetAsync(swaggerRequestUri);
            var swagger = await swaggerResponse.Content.ReadAsStringAsync();
            await Verifier.Verify(swagger).UseParameters(startupType, GetVersion(swaggerRequestUri));
        }

#if NET8_0_OR_GREATER
        [Theory]
        [InlineData("/swagger/v1/swagger.json")]
        public async Task SwaggerEndpoint_ReturnsValidSwaggerJson_For_WebApi(
            string swaggerRequestUri)
        {
            var swaggerResponse = await SwaggerEndpointReturnsValidSwaggerJson<WebApi.Program>(swaggerRequestUri);
            await Verifier.Verify(swaggerResponse).UseParameters(GetVersion(swaggerRequestUri));
        }

        [Theory]
        [InlineData("/swagger/v1/swagger.json")]
        public async Task SwaggerEndpoint_ReturnsValidSwaggerJson_For_Mvc(
            string swaggerRequestUri)
        {
            var swaggerResponse = await SwaggerEndpointReturnsValidSwaggerJson<MvcWithNullable.Program>(swaggerRequestUri);
            await Verifier.Verify(swaggerResponse).UseParameters(GetVersion(swaggerRequestUri));
        }

        [Fact]
        public async Task TypesAreRenderedCorrectly()
        {
            using var application = new TestApplication<WebApi.Program>();
            using var client = application.CreateDefaultClient();

            var swaggerResponse = await SwaggerResponse(client, "/swagger/v1/swagger.json");
            await Verifier.Verify(swaggerResponse);
        }

        private static async Task<string> SwaggerEndpointReturnsValidSwaggerJson<TEntryPoint>(string swaggerRequestUri)
            where TEntryPoint : class
        {
            using var application = new TestApplication<TEntryPoint>();
            using var client = application.CreateDefaultClient();

            return await SwaggerResponse(client, swaggerRequestUri);
        }
#endif
        private static async Task<string> SwaggerResponse(HttpClient client, string swaggerRequestUri)
        {
            using var swaggerResponse = await client.GetAsync(swaggerRequestUri);
            var contentStream = await swaggerResponse.Content.ReadAsStringAsync();
            return contentStream;
        }
        private static string GetVersion(string swaggerUi) => Regex.Match(swaggerUi, "/\\w+/([\\w+\\d+.-]+)/").Groups[1].Value;
    }
}
