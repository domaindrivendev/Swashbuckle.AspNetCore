using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.OpenApi.Models;
using Xunit;

namespace Swashbuckle.AspNetCore.ApiTesting.Test
{
    public class RequestValidatorTests
    {
        [Theory]
        [InlineData("/api/foobar", "/api/products", "Request URI '/api/foobar' does not match specified template '/api/products'")]
        [InlineData("/api/products", "/api/products", null)]
        public void Validate_ThrowsException_IfUriDoesNotMatchPathTemplate(
            string uriString,
            string pathTemplate,
            string expectedErrorMessage)
        {
            var openApiDocument = DocumentWithOperation(pathTemplate, OperationType.Get, new OpenApiOperation());
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(uriString, UriKind.Relative),
            };

            var exception = Record.Exception(() =>
            {
                Subject().Validate(request, openApiDocument, pathTemplate, OperationType.Get);
            });

            Assert.Equal(expectedErrorMessage, exception?.Message);
        }

        [Theory]
        [InlineData("POST", OperationType.Get, "Request method 'POST' does not match specified operation type 'Get'")]
        [InlineData("GET", OperationType.Get, null)]
        public void Validate_ThrowsException_IfMethodDoesNotMatchOperationType(
            string methodString,
            OperationType operationType,
            string expectedErrorMessage)
        {
            var openApiDocument = DocumentWithOperation("/api/products", operationType, new OpenApiOperation());
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("/api/products", UriKind.Relative),
                Method = new HttpMethod(methodString)
            };

            var exception = Record.Exception(() =>
            {
                Subject().Validate(request, openApiDocument, "/api/products", operationType);
            });

            Assert.Equal(expectedErrorMessage, exception?.Message);
        }

        [Theory]
        [InlineData("/api/products", "Required parameter 'param' is not present")]
        [InlineData("/api/products?param=foo", null)]
        public void Validate_ThrowsException_IfRequiredQueryParameterIsNotPresent(
            string uriString,
            string expectedErrorMessage)
        {
            var openApiDocument = DocumentWithOperation("/api/products", OperationType.Get, new OpenApiOperation
            {
                Parameters = new List<OpenApiParameter>
                {
                    new OpenApiParameter
                    {
                        Name = "param",
                        In = ParameterLocation.Query,
                        Schema = new OpenApiSchema { Type = "string" },
                        Required = true
                    }
                }
            });
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(uriString, UriKind.Relative),
                Method = HttpMethod.Get
            };

            var exception = Record.Exception(() =>
            {
                Subject().Validate(request, openApiDocument, "/api/products", OperationType.Get);
            });

            Assert.Equal(expectedErrorMessage, exception?.Message);
        }

        [Theory]
        [InlineData(null, "Required parameter 'test-header' is not present")]
        [InlineData("foo", null)]
        public void Validate_ThrowsException_IfRequiredHeaderParameterIsNotPresent(
            string parameterValue,
            string expectedErrorMessage)
        {
            var openApiDocument = DocumentWithOperation("/api/products", OperationType.Get, new OpenApiOperation
            {
                Parameters = new List<OpenApiParameter>
                {
                    new OpenApiParameter
                    {
                        Name = "test-header",
                        In = ParameterLocation.Header,
                        Schema = new OpenApiSchema { Type = "string" },
                        Required = true
                    }
                }
            });
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("/api/products", UriKind.Relative),
                Method = HttpMethod.Get,
            };
            if (parameterValue != null) request.Headers.Add("test-header", parameterValue);

            var exception = Record.Exception(() =>
            {
                Subject().Validate(request, openApiDocument, "/api/products", OperationType.Get);
            });

            Assert.Equal(expectedErrorMessage, exception?.Message);
        }


        [Theory]
        [InlineData("/api/products/foo", "boolean", "Parameter 'param' is not of type 'boolean'")]
        [InlineData("/api/products/foo", "number", "Parameter 'param' is not of type 'number'")]
        [InlineData("/api/products/true", "boolean", null)]
        [InlineData("/api/products/1", "number", null)]
        [InlineData("/api/products/foo", "string", null)]
        public void Validate_ThrowsException_IfPathParameterIsNotOfSpecifiedType(
            string uriString,
            string specifiedType,
            string expectedErrorMessage)
        {
            var openApiDocument = DocumentWithOperation("/api/products/{param}", OperationType.Get, new OpenApiOperation
            {
                Parameters = new List<OpenApiParameter>
                {
                    new OpenApiParameter
                    {
                        Name = "param",
                        In = ParameterLocation.Path,
                        Schema = new OpenApiSchema { Type = specifiedType }
                    }
                }
            });
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(uriString, UriKind.Relative),
                Method = HttpMethod.Get
            };

            var exception = Record.Exception(() =>
            {
                Subject().Validate(request, openApiDocument, "/api/products/{param}", OperationType.Get);
            });

            Assert.Equal(expectedErrorMessage, exception?.Message);
        }

        [Theory]
        [InlineData("/api/products?param=foo", "boolean", null, "Parameter 'param' is not of type 'boolean'")]
        [InlineData("/api/products?param=foo", "number", null, "Parameter 'param' is not of type 'number'")]
        [InlineData("/api/products?param=1&param=foo", "array", "number", "Parameter 'param' is not of type 'array[number]'")]
        [InlineData("/api/products?param=true", "boolean", null, null)]
        [InlineData("/api/products?param=1", "number", null, null)]
        [InlineData("/api/products?param=foo", "string", null, null)]
        [InlineData("/api/products?param=1&param=2", "array", "number", null)]
        public void Validate_ThrowsException_IfQueryParameterIsNotOfSpecifiedType(
            string path,
            string specifiedType,
            string specifiedItemsType,
            string expectedErrorMessage)
        {
            var openApiDocument = DocumentWithOperation("/api/products", OperationType.Get, new OpenApiOperation
            {
                Parameters = new List<OpenApiParameter>
                {
                    new OpenApiParameter
                    {
                        Name = "param",
                        In = ParameterLocation.Query,
                        Schema = new OpenApiSchema
                        {
                            Type = specifiedType,
                            Items = (specifiedItemsType != null) ? new OpenApiSchema { Type = specifiedItemsType } : null
                        }
                    }
                }
            });
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(path, UriKind.Relative),
                Method = HttpMethod.Get
            };

            var exception = Record.Exception(() =>
            {
                Subject().Validate(request, openApiDocument, "/api/products", OperationType.Get);
            });

            Assert.Equal(expectedErrorMessage, exception?.Message);
        }

        [Theory]
        [InlineData("foo", "boolean", null, "Parameter 'test-header' is not of type 'boolean'")]
        [InlineData("foo", "number", null, "Parameter 'test-header' is not of type 'number'")]
        [InlineData("1,foo", "array", "number", "Parameter 'test-header' is not of type 'array[number]'")]
        [InlineData("true", "boolean", null, null)]
        [InlineData("1", "number", null, null)]
        [InlineData("foo", "string", null, null)]
        [InlineData("1,2", "array", "number", null)]
        public void Validate_ThrowsException_IfHeaderParameterIsNotOfSpecifiedType(
            string parameterValue,
            string specifiedType,
            string specifiedItemsType,
            string expectedErrorMessage)
        {
            var openApiDocument = DocumentWithOperation("/api/products", OperationType.Get, new OpenApiOperation
            {
                Parameters = new List<OpenApiParameter>
                {
                    new OpenApiParameter
                    {
                        Name = "test-header",
                        In = ParameterLocation.Header,
                        Schema = new OpenApiSchema
                        {
                            Type = specifiedType,
                            Items = (specifiedItemsType != null) ? new OpenApiSchema { Type = specifiedItemsType } : null
                        }
                    }
                }
            });
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("/api/products", UriKind.Relative),
                Method = HttpMethod.Get
            };
            if (parameterValue != null) request.Headers.Add("test-header", parameterValue);

            var exception = Record.Exception(() =>
            {
                Subject().Validate(request, openApiDocument, "/api/products", OperationType.Get);
            });

            Assert.Equal(expectedErrorMessage, exception?.Message);
        }

        [Theory]
        [InlineData(null, "Required content is not present")]
        [InlineData("foo", null)]
        public void Validate_ThrowsException_IfRequiredContentIsNotPresent(
            string contentString,
            string expectedErrorMessage)
        {
            var openApiDocument = DocumentWithOperation("/api/products", OperationType.Post, new OpenApiOperation
            {
                RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        [ "text/plain" ] = new OpenApiMediaType()
                    }
                }
            });
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("/api/products", UriKind.Relative),
                Method = HttpMethod.Post
            };
            if (contentString != null) request.Content = new StringContent(contentString);

            var exception = Record.Exception(() =>
            {
                Subject().Validate(request, openApiDocument, "/api/products", OperationType.Post);
            });

            Assert.Equal(expectedErrorMessage, exception?.Message);
        }

        [Theory]
        [InlineData("application/foo", "Content media type 'application/foo' is not specified")]
        [InlineData("application/json", null)]
        public void Validate_ThrowsException_IfContentMediaTypeIsNotSpecified(
            string mediaType,
            string expectedErrorMessage)
        {
            var openApiDocument = DocumentWithOperation("/api/products", OperationType.Post, new OpenApiOperation
            {
                RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        [ "application/json" ] = new OpenApiMediaType()
                    }
                }
            });
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("/api/products", UriKind.Relative),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"foo\":\"bar\"}", Encoding.UTF8, mediaType) 
            };

            var exception = Record.Exception(() =>
            {
                Subject().Validate(request, openApiDocument, "/api/products", OperationType.Post);
            });

            Assert.Equal(expectedErrorMessage, exception?.Message);
        }

        [Theory]
        [InlineData("{\"prop1\":\"foo\"}", "Content does not match spec. Path: . Required property(s) not present")]
        [InlineData("{\"prop1\":\"foo\",\"prop2\":\"bar\"}", null)]
        public void Validate_DelegatesContentValidationToInjectedContentValidators(
            string jsonString,
            string expectedErrorMessage)
        {
            var openApiDocument = DocumentWithOperation("/api/products", OperationType.Post, new OpenApiOperation
            {
                RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        [ "application/json" ] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Required = new SortedSet<string> { "prop1", "prop2" }
                            }
                        }
                    }
                }
            });
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("/api/products", UriKind.Relative),
                Method = HttpMethod.Post,
                Content = new StringContent(jsonString, Encoding.UTF8, "application/json") 
            };

            var exception = Record.Exception(() =>
            {
                Subject(new[] { new JsonContentValidator() }).Validate(request, openApiDocument, "/api/products", OperationType.Post);
            });

            Assert.Equal(expectedErrorMessage, exception?.Message);
        }

        private OpenApiDocument DocumentWithOperation(string pathTemplate, OperationType operationType, OpenApiOperation operationSpec)
        {
            return new OpenApiDocument
            {
                Paths = new OpenApiPaths
                {
                    [pathTemplate] = new OpenApiPathItem
                    {
                        Operations = new Dictionary<OperationType, OpenApiOperation>
                        {
                            [operationType] = operationSpec
                        }
                    }
                },
                Components = new OpenApiComponents
                {
                    Schemas = new Dictionary<string, OpenApiSchema>()
                }
            };
        }

        private RequestValidator Subject(IEnumerable<IContentValidator> contentValidators = null)
        {
            return new RequestValidator(contentValidators ?? new IContentValidator[] { });
        }
    }
}
