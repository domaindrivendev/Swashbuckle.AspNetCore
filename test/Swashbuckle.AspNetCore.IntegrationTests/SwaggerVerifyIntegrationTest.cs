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
    public partial class SwaggerVerifyIntegrationTest
    {
        [Theory]
#if !NET6_0
        [InlineData(typeof(Basic.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(NSwagClientExample.Startup), "/swagger/v1/swagger.json")]
#endif
        [InlineData(typeof(CliExample.Startup), "/swagger/v1/swagger_net8.0.json")]
        [InlineData(typeof(ConfigFromFile.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(CustomDocumentSerializer.Startup), "/swagger/v1/swagger.json")]
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

        [Fact]
        public async Task SwaggerEndpoint_ReturnsValidSwaggerJson_ForAutofaq()
        {
            var startupType = typeof(CliExampleWithFactory.Startup);
            const string swaggerRequestUri = "/swagger/v1/swagger_net8.0.json";

            var testSite = new TestSiteAutofaq(startupType);
            using var client = testSite.BuildClient();

            using var swaggerResponse = await client.GetAsync(swaggerRequestUri);
            var swagger = await swaggerResponse.Content.ReadAsStringAsync();
            await Verifier.Verify(swagger).UseParameters(startupType, GetVersion(swaggerRequestUri));
        }

#if NET6_0
        [Theory]
        [InlineData(typeof(Basic.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(NSwagClientExample.Startup), "/swagger/v1/swagger.json")]
        public async Task SwaggerEndpoint_ReturnsValidSwaggerJson_DotNet6(
            Type startupType,
            string swaggerRequestUri)
        {
            var testSite = new TestSite(startupType);
            using var client = testSite.BuildClient();

            using var swaggerResponse = await client.GetAsync(swaggerRequestUri);
            var swagger = await swaggerResponse.Content.ReadAsStringAsync();
            await Verifier.Verify(swagger).UseParameters(startupType, GetVersion(swaggerRequestUri));
        }
#endif

        [Theory]
        [InlineData(typeof(MinimalApp.Program), "/swagger/v1/swagger.json")]
        [InlineData(typeof(MinimalAppHostedServ.Program), "/swagger/v1/swagger.json")]
        [InlineData(typeof(TopLevelSwaggerDoc.Program), "/swagger/v1.json")]
#if NET8_0_OR_GREATER
        [InlineData(typeof(MvcWithNullable.Program), "/swagger/v1/swagger.json")]
        [InlineData(typeof(WebApi.Program), "/swagger/v1/swagger.json")]
        [InlineData(typeof(WebApi.Aot.Program), "/swagger/v1/swagger.json")]
#endif
        public async Task SwaggerEndpoint_ReturnsValidSwaggerJson_Without_Startup(
            Type entryPointType,
            string swaggerRequestUri)
        {
            var swaggerResponse = await SwaggerEndpointReturnsValidSwaggerJson(entryPointType, swaggerRequestUri);
            await Verifier.Verify(swaggerResponse).UseParameters(entryPointType, GetVersion(swaggerRequestUri));
        }

#if NET8_0_OR_GREATER
        [Fact]
        public async Task TypesAreRenderedCorrectly()
        {
            using var application = new TestApplication<WebApi.Program>();
            using var client = application.CreateDefaultClient();

            var swaggerResponse = await SwaggerResponse(client, "/swagger/v1/swagger.json");
            await Verifier.Verify(swaggerResponse);
        }
#endif

        private static async Task<string> SwaggerEndpointReturnsValidSwaggerJson(Type entryPointType, string swaggerRequestUri)
        {
            using var client = SwaggerIntegrationTests.GetHttpClientForTestApplication(entryPointType);
            return await SwaggerResponse(client, swaggerRequestUri);
        }

        private static async Task<string> SwaggerResponse(HttpClient client, string swaggerRequestUri)
        {
            using var swaggerResponse = await client.GetAsync(swaggerRequestUri);
            var contentStream = await swaggerResponse.Content.ReadAsStringAsync();
            return contentStream;
        }

        private static string GetVersion(string swaggerUi) =>
#if NET6_0
            Regex.Match(swaggerUi, "/\\w+/([\\w+\\d+.-]+)/").Groups[1].Value;
#else
            VersionRegex().Match(swaggerUi).Groups[1].Value;

        [GeneratedRegex("/\\w+/([\\w+\\d+.-]+)/")]
        private static partial Regex VersionRegex();
#endif
    }
}
