using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.OpenApi.Models;
using Xunit;

namespace Swashbuckle.AspNetCore.ApiTesting.Xunit
{
    [Collection("ApiTests")]
    public class ApiTestFixture<TEntryPoint> :
        IClassFixture<WebApplicationFactory<TEntryPoint>> where TEntryPoint : class
    {
        private readonly ApiTestRunnerBase _apiTestRunner;
        private readonly WebApplicationFactory<TEntryPoint> _webAppFactory;
        private readonly string _documentName;

        public ApiTestFixture(
            ApiTestRunnerBase apiTestRunner,
            WebApplicationFactory<TEntryPoint> webAppFactory,
            string documentName)
        {
            _apiTestRunner = apiTestRunner;
            _webAppFactory = webAppFactory;
            _documentName = documentName;
        }

        public void Describe(string pathTemplate, OperationType operationType, OpenApiOperation operationSpec)
        {
            _apiTestRunner.ConfigureOperation(_documentName, pathTemplate, operationType, operationSpec);
        }

        public async Task TestAsync(string operationId, string expectedStatusCode, HttpRequestMessage request)
        {
            await _apiTestRunner.TestAsync(
                _documentName,
                operationId,
                expectedStatusCode,
                request,
                _webAppFactory.CreateClient());
        }
    }
}