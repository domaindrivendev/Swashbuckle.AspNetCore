using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Xunit;

namespace Swashbuckle.AspNetCore.ApiTesting.Test
{
    public class ApiTestRunnerBaseTests
    {
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
                            Operations = new Dictionary<OperationType, OpenApiOperation>
                            {
                                [OperationType.Get] = new OpenApiOperation
                                {
                                    OperationId = "GetProducts",
                                    Parameters = new List<OpenApiParameter>
                                    {
                                        new OpenApiParameter
                                        {
                                            Name = "param",
                                            Required = true,
                                            In = ParameterLocation.Query
                                        }
                                    },
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
                            Operations = new Dictionary<OperationType, OpenApiOperation>
                            {
                                [OperationType.Get] = new OpenApiOperation
                                {
                                    OperationId = "GetProducts",
                                    Responses = new OpenApiResponses
                                    {
                                        [ "400" ] = new OpenApiResponse(),
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

        private FakeApiTestRunner Subject()
        {
            return new FakeApiTestRunner();
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient(new FakeHttpMessageHandler());
            client.BaseAddress = new Uri("http://tempuri.org");
            return client;
        }
    }

    internal class FakeApiTestRunner : ApiTestRunnerBase
    {
        public FakeApiTestRunner()
        {
        }
    }

    internal class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage());
        }
    }
}
