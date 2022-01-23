using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Readers;
using Xunit;
using ReDocApp = ReDoc;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
    public class SwaggerIntegrationTests
    {
        [Theory]
        [InlineData(typeof(Basic.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(CliExample.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(ConfigFromFile.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(CustomUIConfig.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(CustomUIIndex.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(GenericControllers.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(MultipleVersions.Startup), "/swagger/1.0/swagger.json")]
        [InlineData(typeof(MultipleVersions.Startup), "/swagger/2.0/swagger.json")]
        //[InlineData(typeof(NetCore21.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(OAuth2Integration.Startup), "/resource-server/swagger/v1/swagger.json")]
        [InlineData(typeof(ReDocApp.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(TestFirst.Startup), "/swagger/v1-generated/openapi.json")]
        public async Task SwaggerEndpoint_ReturnsValidSwaggerJson(
            Type startupType,
            string swaggerRequestUri)
        {
            var testSite = new TestSite(startupType);
            var client = testSite.BuildClient();

            var swaggerResponse = await client.GetAsync(swaggerRequestUri);

            swaggerResponse.EnsureSuccessStatusCode();
            var contentStream = await swaggerResponse.Content.ReadAsStreamAsync();
            new OpenApiStreamReader().Read(contentStream, out OpenApiDiagnostic diagnostic);
            Assert.Empty(diagnostic.Errors);
        }

        [Fact]
        public async Task SwaggerEndpoint_ReturnsNotFound_IfUnknownSwaggerDocument()
        {
            var testSite = new TestSite(typeof(Basic.Startup));
            var client = testSite.BuildClient();

            var swaggerResponse = await client.GetAsync("/swagger/v2/swagger.json");

            Assert.Equal(System.Net.HttpStatusCode.NotFound, swaggerResponse.StatusCode);
        }

        [Fact]
        public async Task SwaggerEndpoint_DoesNotReturnByteOrderMark()
        {
            var testSite = new TestSite(typeof(Basic.Startup));
            var client = testSite.BuildClient();

            var swaggerResponse = await client.GetAsync("/swagger/v1/swagger.json");

            swaggerResponse.EnsureSuccessStatusCode();
            var contentBytes = await swaggerResponse.Content.ReadAsByteArrayAsync();
            var bomBytes = Encoding.UTF8.GetPreamble();
            Assert.NotEqual(bomBytes, contentBytes.Take(bomBytes.Length));
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("sv-SE")]
        public async Task SwaggerEndpoint_ReturnsCorrectPriceExample_ForDifferentCultures(string culture)
        {
            var testSite = new TestSite(typeof(Basic.Startup));
            var client = testSite.BuildClient();

            var swaggerResponse = await client.GetAsync($"/swagger/v1/swagger.json?culture={culture}");

            swaggerResponse.EnsureSuccessStatusCode();
            var contentStream = await swaggerResponse.Content.ReadAsStreamAsync();
            var currentCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            try
            {
                var openApiDocument = new OpenApiStreamReader().Read(contentStream, out OpenApiDiagnostic diagnostic);
                var example = openApiDocument.Components.Schemas["Product"].Example as OpenApiObject;
                var price = (example["price"] as OpenApiDouble);
                Assert.NotNull(price);
                Assert.Equal(14.37, price.Value);
            }
            finally
            {
                CultureInfo.CurrentCulture = currentCulture;
            }
        }

        [Theory]
        [InlineData("/swagger/v1/swagger.json", "openapi", "3.0.1")]
        [InlineData("/swagger/v1/swaggerv2.json", "swagger", "2.0")]
        public async Task SwaggerMiddleware_CanBeConfiguredMultipleTimes(
            string swaggerUrl,
            string expectedVersionProperty,
            string expectedVersionValue)
        {
            var client = new TestSite(typeof(Basic.Startup)).BuildClient();

            var response = await client.GetAsync(swaggerUrl);

            response.EnsureSuccessStatusCode();
            var contentStream = await response.Content.ReadAsStreamAsync();

            var json = await JsonSerializer.DeserializeAsync<JsonElement>(contentStream);
            Assert.Equal(expectedVersionValue, json.GetProperty(expectedVersionProperty).GetString());
        }
    }
}
