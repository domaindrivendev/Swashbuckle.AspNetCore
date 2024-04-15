﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        private readonly IAsyncSwaggerProvider _swaggerProvider;
        private readonly ISwaggerDocumentSerializer _swaggerDocumentSerializer;

        public DocumentProvider(
            IOptions<SwaggerGeneratorOptions> generatorOptions,
            IOptions<SwaggerOptions> options,
            IAsyncSwaggerProvider swaggerProvider
            ) : this(generatorOptions, options, swaggerProvider, null)
        { }

        public DocumentProvider(
            IOptions<SwaggerGeneratorOptions> generatorOptions,
            IOptions<SwaggerOptions> options,
            IAsyncSwaggerProvider swaggerProvider,
            IServiceProvider serviceProvider
            )
        {
            _generatorOptions = generatorOptions.Value;
            _options = options.Value;
            _swaggerProvider = swaggerProvider;

            // Use IServiceProvider to retrieve the ISwaggerDocumentSerializer, because it is an optional service
            _swaggerDocumentSerializer = serviceProvider?.GetService<ISwaggerDocumentSerializer>();
        }

        public IEnumerable<string> GetDocumentNames()
        {
            return _generatorOptions.SwaggerDocs.Keys;
        }

        public async Task GenerateAsync(string documentName, TextWriter writer)
        {
            // Let UnknownSwaggerDocument or other exception bubble up to caller.
            var swagger = await _swaggerProvider.GetSwaggerAsync(documentName, host: null, basePath: null);
            var jsonWriter = new OpenApiJsonWriter(writer);
            if (_options.SerializeAsV2)
            {
                if (_swaggerDocumentSerializer != null)
                    _swaggerDocumentSerializer.SerializeDocument(swagger, jsonWriter, OpenApi.OpenApiSpecVersion.OpenApi2_0);
                else
                    swagger.SerializeAsV2(jsonWriter);
            }
            else
            {
                if (_swaggerDocumentSerializer != null)
                    _swaggerDocumentSerializer.SerializeDocument(swagger, jsonWriter, OpenApi.OpenApiSpecVersion.OpenApi3_0);
                else
                    swagger.SerializeAsV3(jsonWriter);
            }
        }
    }
}
