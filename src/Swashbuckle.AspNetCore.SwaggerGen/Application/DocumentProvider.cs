using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;

namespace Microsoft.Extensions.ApiDescriptions
{
    /// <summary>
    /// This service will be looked up by name from the service collection when using
    /// the Microsoft.Extensions.ApiDescription tool
    /// </summary>
    internal interface IDocumentProvider
    {
        Task GenerateAsync(string documentName, TextWriter writer);
    }

    internal class DocumentProvider : IDocumentProvider
    {
        private readonly SwaggerOptions _options;
        private readonly ISwaggerProvider _swaggerProvider;

        public DocumentProvider(IOptions<SwaggerOptions> options, ISwaggerProvider swaggerProvider)
        {
            _options = options.Value;
            _swaggerProvider = swaggerProvider;
        }

        public Task GenerateAsync(string documentName, TextWriter writer)
        {
            // Let UnknownSwaggerDocument or other exception bubble up to caller.
            var swagger = _swaggerProvider.GetSwagger(documentName, host: null, basePath: null);
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
