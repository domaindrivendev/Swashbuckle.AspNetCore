using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public abstract class ApiTestRunnerBase : IDisposable
    {
        private readonly ApiTestRunnerOptions _options;
        private readonly RequestValidator _requestValidator;
        private readonly ResponseValidator _responseValidator;

        protected ApiTestRunnerBase()
        {
            _options = new ApiTestRunnerOptions();
            _requestValidator = new RequestValidator(_options.ContentValidators);
            _responseValidator = new ResponseValidator(_options.ContentValidators);
        }

        public void Configure(Action<ApiTestRunnerOptions> setupAction)
        {
            setupAction(_options);
        }

        public void ConfigureOperation(
            string documentName,
            string pathTemplate,
            OperationType operationType,
            OpenApiOperation operation)
        {
            var openApiDocument = _options.GetOpenApiDocument(documentName);

            if (openApiDocument.Paths == null)
                openApiDocument.Paths = new OpenApiPaths();

            if (!openApiDocument.Paths.TryGetValue(pathTemplate, out OpenApiPathItem pathItem))
            {
                pathItem = new OpenApiPathItem();
                openApiDocument.Paths.Add(pathTemplate, pathItem);
            }

            pathItem.AddOperation(operationType, operation);
        }

        public async Task TestAsync(
            string documentName,
            string operationId,
            string expectedStatusCode,
            HttpRequestMessage request,
            HttpClient httpClient)
        {
            var openApiDocument = _options.GetOpenApiDocument(documentName);
            if (!openApiDocument.TryFindOperationById(operationId, out string pathTemplate, out OperationType operationType))
                throw new InvalidOperationException($"Operation with id '{operationId}' not found in OpenAPI document '{documentName}'");

            if (expectedStatusCode.StartsWith("2"))
                _requestValidator.Validate(request, openApiDocument, pathTemplate, operationType);

            var response = await httpClient.SendAsync(request);

            _responseValidator.Validate(response, openApiDocument, pathTemplate, operationType, expectedStatusCode);
        }

        public void Dispose()
        {
            if (!_options.GenerateOpenApiFiles) return;

            if (_options.FileOutputRoot == null)
                throw new Exception("GenerateOpenApiFiles set but FileOutputRoot is null");

            foreach (var entry in _options.OpenApiDocs)
            {
                var outputDir = Path.Combine(_options.FileOutputRoot, entry.Key);
                Directory.CreateDirectory(outputDir);

                using (var streamWriter = new StreamWriter(Path.Combine(outputDir, "openapi.json")))
                {
                    var openApiJsonWriter = new OpenApiJsonWriter(streamWriter);
                    entry.Value.SerializeAsV3(openApiJsonWriter);
                    streamWriter.Close();
                }
            }
        }
    }
}