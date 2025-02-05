using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.OpenApi.Models;
using Xunit;

namespace Swashbuckle.AspNetCore.ApiTesting.Test
{
    public class ResponseValidatorTests
    {
        [Theory]
        [InlineData(HttpStatusCode.InternalServerError, "Status code '500' does not match expected value '200'")]
        [InlineData(HttpStatusCode.OK, null)]
        public void Validate_ThrowsException_IfStatusCodeDifferentToExpectedResponseCode(
            HttpStatusCode statusCode,
            string expectedErrorMessage)
        {
            var openApiDocument = DocumentWithOperation("/api/products", OperationType.Get, new OpenApiOperation
            {
                Responses = new OpenApiResponses
                {
                    [ "200" ] = new OpenApiResponse(),
                    [ "500" ] = new OpenApiResponse()
                }
            });
            var response = new HttpResponseMessage
            {
                StatusCode = statusCode
            };

            var exception = Record.Exception(() =>
            {
                Subject().Validate(response, openApiDocument, "/api/products", OperationType.Get, "200");
            });

            Assert.Equal(expectedErrorMessage, exception?.Message);
        }

        [Theory]
        [InlineData(null, "Required header 'test-header' is not present")]
        [InlineData("foo", null)]
        public void Validate_ThrowsException_IfRequiredHeaderIsNotPresent(
            string headerValue,
            string expectedErrorMessage)
        {
            var openApiDocument = DocumentWithOperation("/api/products", OperationType.Post, new OpenApiOperation
            {
                Responses = new OpenApiResponses
                {
                    [ "201" ] = new OpenApiResponse
                    {
                        Headers = new Dictionary<string, OpenApiHeader>
                        {
                            [ "test-header" ] = new OpenApiHeader
                            {
                                Required = true
                            }
                        }
                    }
                }
            });
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created
            };
            if (headerValue != null) response.Headers.Add("test-header", headerValue);

            var exception = Record.Exception(() =>
            {
                Subject().Validate(response, openApiDocument, "/api/products", OperationType.Post, "201");
            });

            Assert.Equal(expectedErrorMessage, exception?.Message);
        }

        [Theory]
        [InlineData("foo", JsonSchemaType.Boolean, null, "Header 'test-header' is not of type 'boolean'")]
        [InlineData("foo", JsonSchemaType.Number, null, "Header 'test-header' is not of type 'number'")]
        [InlineData("1,foo", JsonSchemaType.Array, JsonSchemaType.Number, "Header 'test-header' is not of type 'array[Number]'")]
        [InlineData("true", JsonSchemaType.Boolean, null, null)]
        [InlineData("1", JsonSchemaType.Number, null, null)]
        [InlineData("foo", JsonSchemaType.String, null, null)]
        [InlineData("1,2", JsonSchemaType.Array, JsonSchemaType.Number, null)]
        public void Validate_ThrowsException_IfHeaderIsNotOfSpecifiedType(
            string headerValue,
            JsonSchemaType specifiedType,
            JsonSchemaType? specifiedItemsType,
            string expectedErrorMessage)
        {
            var openApiDocument = DocumentWithOperation("/api/products", OperationType.Post, new OpenApiOperation
            {
                Responses = new OpenApiResponses
                {
                    [ "201" ] = new OpenApiResponse
                    {
                        Headers = new Dictionary<string, OpenApiHeader>
                        {
                            [ "test-header" ] = new OpenApiHeader
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = specifiedType,
                                    Items = (specifiedItemsType != null) ? new OpenApiSchema { Type = specifiedItemsType } : null
                                }
                            }
                        }
                    }
                }
            });
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created
            };
            response.Headers.Add("test-header", headerValue);

            var exception = Record.Exception(() =>
            {
                Subject().Validate(response, openApiDocument, "/api/products", OperationType.Post, "201");
            });

            Assert.Equal(expectedErrorMessage, exception?.Message);
        }

        [Theory]
        [InlineData(null, "Expected content is not present")]
        [InlineData("foo", null)]
        public void Validate_ThrowsException_IfExpectedContentIsNotPresent(
            string contentString,
            string expectedErrorMessage)
        {
            var openApiDocument = DocumentWithOperation("/api/products/1", OperationType.Get, new OpenApiOperation
            {
                Responses = new OpenApiResponses
                {
                    [ "200" ] = new OpenApiResponse
                    {
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["text/plain"] = new OpenApiMediaType()
                        }
                    }
                }
            });
            var response = new HttpResponseMessage
            {
            };
            if (contentString != null) response.Content = new StringContent(contentString);

            var exception = Record.Exception(() =>
            {
                Subject().Validate(response, openApiDocument, "/api/products/1", OperationType.Get, "200");
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
            var openApiDocument = DocumentWithOperation("/api/products/1", OperationType.Get, new OpenApiOperation
            {
                Responses = new OpenApiResponses
                {
                    [ "200" ] = new OpenApiResponse
                    {
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType()
                        }
                    }
                }
            });
            var response = new HttpResponseMessage
            {
                Content = new StringContent("{\"foo\":\"bar\"}", Encoding.UTF8, mediaType)
            };

            var exception = Record.Exception(() =>
            {
                Subject().Validate(response, openApiDocument, "/api/products/1", OperationType.Get, "200");
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
            var openApiDocument = DocumentWithOperation("/api/products/1", OperationType.Get, new OpenApiOperation
            {
                Responses = new OpenApiResponses
                {
                    [ "200" ] = new OpenApiResponse
                    {
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = JsonSchemaType.Object,
                                    Required = new SortedSet<string> { "prop1", "prop2" }
                                }
                            }
                        }
                    }
                }
            });
            var response = new HttpResponseMessage
            {
                Content = new StringContent(jsonString, Encoding.UTF8, "application/json") 
            };

            var exception = Record.Exception(() =>
            {
                Subject([new JsonContentValidator()]).Validate(response, openApiDocument, "/api/products/1", OperationType.Get, "200");
            });

            Assert.Equal(expectedErrorMessage, exception?.Message);
        }


        private static OpenApiDocument DocumentWithOperation(string pathTemplate, OperationType operationType, OpenApiOperation operationSpec)
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

        private static ResponseValidator Subject(IEnumerable<IContentValidator> contentValidators = null)
        {
            return new ResponseValidator(contentValidators ?? []);
        }
    }
}
