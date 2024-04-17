using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.ApiDescriptions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Readers;
using Swashbuckle.AspNetCore.Swagger;
using Xunit;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
    public class CustomDocumentSerializerTests
    {
        [Fact]
        public async void CustomDocumentSerializer_Writes_Custom_V3_Document()
        {
            var testSite = new TestSite(typeof(CustomDocumentSerializer.Startup));
            var client = testSite.BuildClient();

            var swaggerResponse = await client.GetAsync($"/swagger/v1/swagger.json");

            swaggerResponse.EnsureSuccessStatusCode();
            var contentStream = await swaggerResponse.Content.ReadAsStreamAsync();
            using var document = JsonDocument.Parse(contentStream);

            // verify that the custom serializer wrote the swagger info
            var swaggerInfo = document.RootElement.GetProperty("swagger").GetString();
            Assert.Equal("DocumentSerializerTest3.0", swaggerInfo);
        }

        [Fact]
        public async void CustomDocumentSerializer_Writes_Custom_V2_Document()
        {
            var testSite = new TestSite(typeof(CustomDocumentSerializer.Startup));
            var client = testSite.BuildClient();

            var swaggerResponse = await client.GetAsync($"/swagger/v1/swaggerv2.json");

            swaggerResponse.EnsureSuccessStatusCode();
            var contentStream = await swaggerResponse.Content.ReadAsStreamAsync();
            using var document = JsonDocument.Parse(contentStream);

            // verify that the custom serializer wrote the swagger info
            var swaggerInfo = document.RootElement.GetProperty("swagger").GetString();
            Assert.Equal("DocumentSerializerTest2.0", swaggerInfo);
        }
    }
}
