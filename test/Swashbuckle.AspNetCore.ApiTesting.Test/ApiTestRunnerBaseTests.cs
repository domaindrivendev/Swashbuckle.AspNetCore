using System.Text.Json;
using Microsoft.OpenApi;
using Xunit;

namespace Swashbuckle.AspNetCore.ApiTesting.Test;

public class ApiTestRunnerBaseTests
{
    [Fact]
    public void Dispose_WritesOpenApiFiles_AsOpenApi3_0_ByDefault()
    {
        var outputRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            var subject = new FakeApiTestRunner();
            subject.Configure(c =>
            {
                c.GenerateOpenApiFiles = true;
                c.FileOutputRoot = outputRoot;
                c.OpenApiDocs.Add("v1", CreateDocument());
            });

            subject.Dispose();

            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(outputRoot, "v1", "openapi.json")));
            Assert.StartsWith("3.0.", document.RootElement.GetProperty("openapi").GetString());
        }
        finally
        {
            if (Directory.Exists(outputRoot))
            {
                Directory.Delete(outputRoot, recursive: true);
            }
        }
    }

    [Theory]
    [InlineData(OpenApiSpecVersion.OpenApi2_0, "swagger", "2.0")]
    [InlineData(OpenApiSpecVersion.OpenApi3_0, "openapi", "3.0.")]
    [InlineData(OpenApiSpecVersion.OpenApi3_1, "openapi", "3.1.")]
    [InlineData(OpenApiSpecVersion.OpenApi3_2, "openapi", "3.2.")]
    public void Dispose_WritesOpenApiFiles_UsingConfiguredVersion(
        OpenApiSpecVersion version,
        string expectedPropertyName,
        string expectedVersionPrefix)
    {
        var outputRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            var subject = new FakeApiTestRunner();
            subject.Configure(c =>
            {
                c.GenerateOpenApiFiles = true;
                c.FileOutputRoot = outputRoot;
                c.OpenApiVersion = version;
                c.OpenApiDocs.Add("v1", CreateDocument());
            });

            subject.Dispose();

            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(outputRoot, "v1", "openapi.json")));
            Assert.StartsWith(expectedVersionPrefix, document.RootElement.GetProperty(expectedPropertyName).GetString());
        }
        finally
        {
            if (Directory.Exists(outputRoot))
            {
                Directory.Delete(outputRoot, recursive: true);
            }
        }
    }
    [Fact]
    public async Task TestAsync_ThrowsException_IfDocumentNotFound()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Subject().TestAsync(
            "v1",
            "GetProducts",
            "200",
            new HttpRequestMessage(),
            CreateHttpClient()));

        Assert.Equal("Document with name 'v1' not found", exception.Message);
    }

    [Fact]
    public async Task TestAsync_ThrowsException_IfOperationNotFound()
    {
        var subject = new FakeApiTestRunner();
        subject.Configure(c =>
        {
            c.OpenApiDocs.Add("v1", new OpenApiDocument());
        });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => subject.TestAsync(
            "v1",
            "GetProducts",
            "200",
            new HttpRequestMessage(),
            CreateHttpClient()));

        Assert.Equal("Operation with id 'GetProducts' not found in OpenAPI document 'v1'", exception.Message);
    }

    [Theory]
    [InlineData("/api/products", "200", "Required parameter 'param' is not present")]
    [InlineData("/api/products?param=foo", "200", null)]
    public async Task TestAsync_ThrowsException_IfExpectedStatusCodeIs2xxAndRequestDoesNotMatchSpec(
        string requestUri,
        string statusCode,
        string expectedExceptionMessage)
    {
        var subject = new FakeApiTestRunner();
        subject.Configure(c =>
        {
            c.OpenApiDocs.Add("v1", new OpenApiDocument
            {
                Paths = new OpenApiPaths
                {
                    ["/api/products"] = new OpenApiPathItem
                    {
                        Operations = new Dictionary<HttpMethod, OpenApiOperation>
                        {
                            [HttpMethod.Get] = new OpenApiOperation
                            {
                                OperationId = "GetProducts",
                                Parameters =
                                [
                                    new OpenApiParameter
                                    {
                                        Name = "param",
                                        Required = true,
                                        In = ParameterLocation.Query
                                    }
                                ],
                                Responses = new OpenApiResponses
                                {
                                    [ "200" ] = new OpenApiResponse()
                                }
                            }
                        }
                    }
                }
            });
        });

        var exception = await Record.ExceptionAsync(() => subject.TestAsync(
            "v1",
            "GetProducts",
            statusCode,
            new HttpRequestMessage
            {
                RequestUri = new Uri(requestUri, UriKind.Relative),
                Method = HttpMethod.Get
            },
            CreateHttpClient()));

        Assert.Equal(expectedExceptionMessage, exception?.Message);
    }

    [Theory]
    [InlineData("/api/products", "400", "Status code '200' does not match expected value '400'")]
    [InlineData("/api/products?param=foo", "200", null)]
    public async Task TestAsync_ThrowsException_IfResponseDoesNotMatchSpec(
        string requestUri,
        string statusCode,
        string expectedExceptionMessage)
    {
        var subject = new FakeApiTestRunner();
        subject.Configure(c =>
        {
            c.OpenApiDocs.Add("v1", new OpenApiDocument
            {
                Paths = new OpenApiPaths
                {
                    ["/api/products"] = new OpenApiPathItem
                    {
                        Operations = new Dictionary<HttpMethod, OpenApiOperation>
                        {
                            [HttpMethod.Get] = new OpenApiOperation
                            {
                                OperationId = "GetProducts",
                                Responses = new OpenApiResponses
                                {
                                    ["400"] = new OpenApiResponse(),
                                    ["200"] = new OpenApiResponse()
                                }
                            }
                        }
                    }
                }
            });
        });

        var exception = await Record.ExceptionAsync(() => subject.TestAsync(
            "v1",
            "GetProducts",
            statusCode,
            new HttpRequestMessage
            {
                RequestUri = new Uri(requestUri, UriKind.Relative),
                Method = HttpMethod.Get
            },
            CreateHttpClient()));

        Assert.Equal(expectedExceptionMessage, exception?.Message);
    }

    private static FakeApiTestRunner Subject()
    {
        return new();
    }

    private static OpenApiDocument CreateDocument()
    {
        return new OpenApiDocument
        {
            Info = new OpenApiInfo { Title = "Test API", Version = "v1" },
            Paths = new OpenApiPaths
            {
                ["/api/products"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new OpenApiOperation
                        {
                            OperationId = "GetProducts",
                            Responses = new OpenApiResponses
                            {
                                ["200"] = new OpenApiResponse { Description = "OK" }
                            }
                        }
                    }
                }
            }
        };
    }

    private static HttpClient CreateHttpClient()
    {
        return new(new FakeHttpMessageHandler())
        {
            BaseAddress = new Uri("http://tempuri.org")
        };
    }
}

internal class FakeApiTestRunner : ApiTestRunnerBase;

internal class FakeHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage());
    }
}
