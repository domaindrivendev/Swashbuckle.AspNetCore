using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
    public class XmlCommentsTests
    {
        [Theory]
        [InlineData(typeof(NewtonsoftConfig.Startup), "/swagger/v1/swagger.json")]
        public async Task SwaggerEndpoint_ReturnsValidSwaggerJson(
            Type startupType,
            string swaggerRequestUri)
        {
            var testSite = new TestSite(startupType);
            var client = testSite.BuildClient();

            var swaggerResponse = await client.GetAsync(swaggerRequestUri);

            swaggerResponse.EnsureSuccessStatusCode();
            var openApiDocument = await GetOpenApiDocument(swaggerResponse);

            var productSchema = openApiDocument.Components.Schemas["Product"];

            Assert.Equal("Product identifier", productSchema.Properties["id"].Description);
            Assert.Equal("Product description", productSchema.Properties["my-description"].Description);
            Assert.Equal("Product status", productSchema.Properties["status"].Description);
            Assert.Equal("Product registration date", productSchema.Properties["registeredOn"].Description);
            Assert.Equal("Product reference", productSchema.Properties["reference"].Description);
        }

        private async Task<OpenApiDocument> GetOpenApiDocument(HttpResponseMessage swaggerResponse)
        {
            var contentStream = await swaggerResponse.Content.ReadAsStreamAsync();

            var openApiDocument = new OpenApiStreamReader().Read(contentStream, out OpenApiDiagnostic diagnostic);

            return openApiDocument;
        }
    }
}
