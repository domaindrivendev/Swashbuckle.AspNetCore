using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.ApiDescriptions
{
    /// <summary>
    /// This service will be looked up by name from the service collection when using
    /// the <c>dotnet-getdocument</c> tool from the Microsoft.Extensions.ApiDescription.Server package.
    /// </summary>
    internal interface IDocumentProvider
    {
        IEnumerable<string> GetDocumentNames();

        Task GenerateAsync(string documentName, TextWriter writer);
    }

    internal class DocumentProvider : IDocumentProvider
    {
        private readonly SwaggerGeneratorOptions _generatorOptions;
        private readonly SwaggerOptions _options;
        private readonly ISwaggerProvider _swaggerProvider;
        private readonly IMemoryCache _memoryCache;

        public DocumentProvider(
            IOptions<SwaggerGeneratorOptions> generatorOptions,
            IOptions<SwaggerOptions> options,
            ISwaggerProvider swaggerProvider,
            IMemoryCache memoryCache)
        {
            _generatorOptions = generatorOptions.Value;
            _options = options.Value;
            _swaggerProvider = swaggerProvider;
            _memoryCache = memoryCache;
        }

        public IEnumerable<string> GetDocumentNames()
        {
            return _generatorOptions.SwaggerDocs.Keys;
        }

        public Task GenerateAsync(string documentName, TextWriter writer)
        {
            // Let UnknownSwaggerDocument or other exception bubble up to caller.
            OpenApiDocument swagger = null;
            if (!_options.UseOpenApiDocumentMemoryCaching)
            {
                swagger = _swaggerProvider.GetSwagger(documentName, host: null, basePath: null);
            }
            else
            {
                if (_memoryCache?.TryGetValue((documentName, (string)null, (string)null), out swagger) != true)
                {
                    swagger = _swaggerProvider.GetSwagger(documentName, host: null, basePath: null);
                    _memoryCache?.Set((documentName, (string)null, (string)null), swagger);
                }
            }
            var jsonWriter = new OpenApiJsonWriter(writer);
            if (_options.SerializeAsV2)
            {
                swagger.SerializeAsV2(jsonWriter);
            }
            else
            {
                swagger.SerializeAsV3(jsonWriter);
            }

            return Task.CompletedTask;
        }
    }
}
