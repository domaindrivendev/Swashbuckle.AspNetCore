﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.ApiDescriptions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.Readers;
using Swashbuckle.AspNetCore.Swagger;
using Xunit;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
    [Collection("TestSite")]
    public class DocumentProviderTests
    {
        static DocumentProviderTests()
        {
            // TODO Make an assembly fixture
            OpenApiReaderRegistry.RegisterReader(OpenApiConstants.Yaml, new OpenApiYamlReader());
        }

        [Theory]
        [InlineData(typeof(Basic.Startup), new[] { "v1" })]
        [InlineData(typeof(CustomUIConfig.Startup), new[] { "v1" })]
        [InlineData(typeof(CustomUIIndex.Startup), new[] { "v1" })]
        [InlineData(typeof(GenericControllers.Startup), new[] { "v1" })]
        [InlineData(typeof(MultipleVersions.Startup), new[] { "1.0", "2.0" })]
        [InlineData(typeof(OAuth2Integration.Startup), new[] { "v1" })]
        public void DocumentProvider_ExposesAllDocumentNames(Type startupType, string[] expectedNames)
        {
            var testSite = new TestSite(startupType);
            var server = testSite.BuildServer();
            var services = server.Host.Services;
            var documentProvider = (IDocumentProvider)services.GetService(typeof(IDocumentProvider));

            var documentNames = documentProvider.GetDocumentNames();

            Assert.Equal(expectedNames, documentNames);
        }

        [Theory]
        [InlineData(typeof(Basic.Startup), "v1")]
        [InlineData(typeof(CustomUIConfig.Startup), "v1")]
        [InlineData(typeof(CustomUIIndex.Startup), "v1")]
        [InlineData(typeof(GenericControllers.Startup), "v1")]
        [InlineData(typeof(MultipleVersions.Startup), "2.0")]
        [InlineData(typeof(OAuth2Integration.Startup), "v1")]
        public async Task DocumentProvider_ExposesGeneratedSwagger(Type startupType, string documentName)
        {
            var testSite = new TestSite(startupType);
            var server = testSite.BuildServer();
            var services = server.Host.Services;

            var documentProvider = (IDocumentProvider)services.GetService(typeof(IDocumentProvider));
            using var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 2048, leaveOpen: true))
            {
                await documentProvider.GenerateAsync(documentName, writer);
                await writer.FlushAsync();
            }

            stream.Position = 0L;
            var result = await OpenApiDocument.LoadAsync(stream);
            Assert.NotNull(result.Diagnostic);
            Assert.Empty(result.Diagnostic.Errors);
        }

        [Fact]
        public async Task DocumentProvider_ThrowsUnknownDocument_IfUnknownDocumentName()
        {
            var testSite = new TestSite(typeof(Basic.Startup));
            var server = testSite.BuildServer();
            var services = server.Host.Services;

            var documentProvider = (IDocumentProvider)services.GetService(typeof(IDocumentProvider));
            using var writer = new StringWriter();
            await Assert.ThrowsAsync<UnknownSwaggerDocument>(
                () => documentProvider.GenerateAsync("NotADocument", writer));
        }
    }
}
