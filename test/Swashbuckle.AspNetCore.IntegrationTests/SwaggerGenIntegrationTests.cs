using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.ApiDescription;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Swashbuckle.AspNetCore.Swagger;
using Xunit;
using Xunit.Abstractions;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
    public class SwaggerGenIntegrationTests
    {
        private readonly ITestOutputHelper _output;

        public SwaggerGenIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
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
            await AssertValidSwaggerAsync(swaggerResponse);
        }

        [Theory]
        [InlineData(typeof(Basic.Startup), "v1")]
        [InlineData(typeof(CustomUIConfig.Startup), "v1")]
        [InlineData(typeof(CustomUIIndex.Startup), "v1")]
        [InlineData(typeof(GenericControllers.Startup), "v1")]
        [InlineData(typeof(MultipleVersions.Startup), "v2")]
        [InlineData(typeof(OAuth2Integration.Startup), "v1")]
        public async Task DocumentProvider_WritesValidDocument(Type startupType, string documentName)
        {
            var testSite = new TestSite(startupType);
            var server = testSite.BuildServer();
            var services = server.Host.Services;
            var documentProvider = (IDocumentProvider)services.GetService(typeof(IDocumentProvider));
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 2048, leaveOpen: true))
                {
                    await documentProvider.GenerateAsync(documentName, writer);
                    await writer.FlushAsync();
                }

                stream.Position = 0L;
                new OpenApiStreamReader().Read(stream, out var diagnostic);
                Assert.NotNull(diagnostic);
                Assert.Empty(diagnostic.Errors);
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

        [Fact]
        public async Task DocumentProvider_ThrowsUnknownDocument()
        {
            var testSite = new TestSite(typeof(Basic.Startup));
            var server = testSite.BuildServer();
            var services = server.Host.Services;
            var documentProvider = (IDocumentProvider)services.GetService(typeof(IDocumentProvider));
            using (var writer = new StringWriter())
            {
                await Assert.ThrowsAsync<UnknownSwaggerDocument>(
                    () => documentProvider.GenerateAsync("NotADocument", writer));
            }
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
