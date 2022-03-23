using System.IO;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.ApiTesting;
using Xunit;

namespace TestFirst.IntegrationTests
{
    public class ApiTestRunner : ApiTestRunnerBase
    {
        public ApiTestRunner()
        {
            Configure(c =>
            {
                var apiDocsRoot = Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", "..", "TestFirst", "wwwroot", "swagger");
            
                // This app demonstrates the two differnt workflows that can be used with this library ...

                // 1) Import OpenAPI file(s) from elsewhere (e.g. created via http://editor.swagger.io) 
                c.AddOpenApiFile("v1-imported", $"{apiDocsRoot}/v1-imported/openapi.json");

                // 2) Configure OpenApi document(s), add Operation descriptions with tests, and generate files after test run
                c.OpenApiDocs.Add("v1-generated", new OpenApiDocument
                {
                    Info = new OpenApiInfo
                    {
                        Version = "v1",
                        Title = "Test-first Example API (Generated)",
                    },
                    Paths = new OpenApiPaths()
                });
                c.GenerateOpenApiFiles = true;
                c.FileOutputRoot = apiDocsRoot;
            });
        }
    }

    [CollectionDefinition("ApiTests")]
    public class ApiTestsCollection : ICollectionFixture<ApiTestRunner>
    {}
}