using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Xunit;
using ReDocApp = ReDoc;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
    public class SwaggerGenIntegrationTests
    {
        [Theory]
        [InlineData(typeof(Basic.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(CliExample.Startup), "/api-docs/v1/swagger.json")]
        //[InlineData(typeof(ConfigFromFile.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(CustomUIConfig.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(CustomUIIndex.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(GenericControllers.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(MultipleVersions.Startup), "/swagger/1.0/swagger.json")]
        [InlineData(typeof(MultipleVersions.Startup), "/swagger/2.0/swagger.json")]
        //[InlineData(typeof(NetCore21.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(OAuth2Integration.Startup), "/resource-server/swagger/v1/swagger.json")]
        [InlineData(typeof(ReDocApp.Startup), "/api-docs/v1/swagger.json")]
        [InlineData(typeof(TestFirst.Startup), "/api-docs/v1-generated/openapi.json")]
        public async Task SwaggerEndpoint_ReturnsValidSwaggerJson(
            Type startupType,
            string swaggerRequestUri)
        {
            var testSite = new TestSite(startupType);
            var client = testSite.BuildClient();

            var swaggerResponse = await client.GetAsync(swaggerRequestUri);

            swaggerResponse.EnsureSuccessStatusCode();
            await AssertResponseDoesNotContainByteOrderMark(swaggerResponse);
            await AssertValidSwaggerAsync(swaggerResponse);
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

        [Fact]
        public async Task SwaggerEndpoint_ReturnsNotFound_IfUnknownSwaggerDocument()
        {
            var testSite = new TestSite(typeof(Basic.Startup));
            var client = testSite.BuildClient();

            var swaggerResponse = await client.GetAsync("/swagger/v2/swagger.json");

            Assert.Equal(System.Net.HttpStatusCode.NotFound, swaggerResponse.StatusCode);
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
            var contentStream = await swaggerResponse.Content.ReadAsStreamAsync();

            var openApiDocument = new OpenApiStreamReader().Read(contentStream, out OpenApiDiagnostic diagnostic);

            Assert.Equal(Enumerable.Empty<OpenApiError>(), diagnostic.Errors);
        }
    }
}
