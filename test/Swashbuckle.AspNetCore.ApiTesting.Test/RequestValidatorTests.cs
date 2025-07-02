using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Xunit;

namespace Swashbuckle.AspNetCore.ApiTesting.Test;

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
        var openApiDocument = DocumentWithOperation(pathTemplate, HttpMethod.Get, new OpenApiOperation());
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(uriString, UriKind.Relative),
        };

        var exception = Record.Exception(() =>
        {
            Subject().Validate(request, openApiDocument, pathTemplate, HttpMethod.Get);
        });

        Assert.Equal(expectedErrorMessage, exception?.Message);
    }

    [Theory]
    [InlineData("POST", "GET", "Request method 'POST' does not match specified operation type 'GET'")]
    [InlineData("GET", "GET", null)]
    public void Validate_ThrowsException_IfMethodDoesNotMatchOperationType(
        string methodString,
        string operationType,
        string expectedErrorMessage)
    {
        var openApiDocument = DocumentWithOperation("/api/products", new(operationType), new OpenApiOperation());
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri("/api/products", UriKind.Relative),
            Method = new HttpMethod(methodString)
        };

        var exception = Record.Exception(() =>
        {
            Subject().Validate(request, openApiDocument, "/api/products", new(operationType));
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
        var openApiDocument = DocumentWithOperation("/api/products", HttpMethod.Get, new OpenApiOperation
        {
            Parameters =
            [
                new OpenApiParameter
                {
                    Name = "param",
                    In = ParameterLocation.Query,
                    Schema = new OpenApiSchema { Type = JsonSchemaTypes.String },
                    Required = true
                }
            ]
        });
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(uriString, UriKind.Relative),
            Method = HttpMethod.Get
        };

        var exception = Record.Exception(() =>
        {
            Subject().Validate(request, openApiDocument, "/api/products", HttpMethod.Get);
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
        var openApiDocument = DocumentWithOperation("/api/products", HttpMethod.Get, new OpenApiOperation
        {
            Parameters =
            [
                new OpenApiParameter
                {
                    Name = "test-header",
                    In = ParameterLocation.Header,
                    Schema = new OpenApiSchema { Type = JsonSchemaTypes.String },
                    Required = true
                }
            ]
        });
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri("/api/products", UriKind.Relative),
            Method = HttpMethod.Get,
        };
        if (parameterValue != null) request.Headers.Add("test-header", parameterValue);

        var exception = Record.Exception(() =>
        {
            Subject().Validate(request, openApiDocument, "/api/products", HttpMethod.Get);
        });

        Assert.Equal(expectedErrorMessage, exception?.Message);
    }

    public static TheoryData<string, JsonSchemaType, string> PathParameterTypeMismatchData => new()
    {
        { "/api/products/foo", JsonSchemaTypes.Boolean, "Parameter 'param' is not of type 'boolean'" },
        { "/api/products/foo", JsonSchemaTypes.Number, "Parameter 'param' is not of type 'number'" },
        { "/api/products/true", JsonSchemaTypes.Boolean, null },
        { "/api/products/1", JsonSchemaTypes.Number, null },
        { "/api/products/foo", JsonSchemaTypes.String, null }
    };

    [Theory]
    [MemberData(nameof(PathParameterTypeMismatchData))]
    public void Validate_ThrowsException_IfPathParameterIsNotOfSpecifiedType(
        string uriString,
        JsonSchemaType specifiedType,
        string expectedErrorMessage)
    {
        var openApiDocument = DocumentWithOperation("/api/products/{param}", HttpMethod.Get, new OpenApiOperation
        {
            Parameters =
            [
                new OpenApiParameter
                {
                    Name = "param",
                    In = ParameterLocation.Path,
                    Schema = new OpenApiSchema { Type = specifiedType }
                }
            ]
        });
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(uriString, UriKind.Relative),
            Method = HttpMethod.Get
        };

        var exception = Record.Exception(() =>
        {
            Subject().Validate(request, openApiDocument, "/api/products/{param}", HttpMethod.Get);
        });

        Assert.Equal(expectedErrorMessage, exception?.Message);
    }

#nullable enable
    public static TheoryData<string, JsonSchemaType, JsonSchemaType?, string?> QueryParameterTypeMismatchData => new()
    {
        { "/api/products?param=foo", JsonSchemaTypes.Boolean, null, "Parameter 'param' is not of type 'boolean'" },
        { "/api/products?param=foo", JsonSchemaTypes.Number, null, "Parameter 'param' is not of type 'number'" },
        { "/api/products?param=true", JsonSchemaTypes.Boolean, null, null },
        { "/api/products?param=1", JsonSchemaTypes.Number, null, null },
        { "/api/products?param=foo", JsonSchemaTypes.String, null, null },
        { "/api/products?param=1&param=2", JsonSchemaTypes.Array, JsonSchemaTypes.Number, null },
        { "/api/products?param=1&param=foo", JsonSchemaTypes.Array, JsonSchemaTypes.Number, "Parameter 'param' is not of type 'array[Number]'" },
    };

    [Theory]
    [MemberData(nameof(QueryParameterTypeMismatchData))]
    public void Validate_ThrowsException_IfQueryParameterIsNotOfSpecifiedType(
        string path,
        JsonSchemaType specifiedType,
        JsonSchemaType? specifiedItemsType,
        string? expectedErrorMessage)
    {
        var openApiDocument = DocumentWithOperation("/api/products", HttpMethod.Get, new OpenApiOperation
        {
            Parameters =
            [
                new OpenApiParameter
                {
                    Name = "param",
                    In = ParameterLocation.Query,
                    Schema = new OpenApiSchema
                    {
                        Type = specifiedType,
                        Items = specifiedItemsType != null ? new OpenApiSchema { Type = specifiedItemsType } : null
                    }
                }
            ]
        });
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(path, UriKind.Relative),
            Method = HttpMethod.Get
        };

        var exception = Record.Exception(() =>
        {
            Subject().Validate(request, openApiDocument, "/api/products", HttpMethod.Get);
        });

        Assert.Equal(expectedErrorMessage, exception?.Message);
    }

    public static TheoryData<string, JsonSchemaType, JsonSchemaType?, string?> HeaderParameterTypeMismatchData => new()
    {
        { "foo", JsonSchemaTypes.Boolean, null, "Parameter 'test-header' is not of type 'boolean'" },
        { "foo", JsonSchemaTypes.Number, null, "Parameter 'test-header' is not of type 'number'" },
        { "true", JsonSchemaTypes.Boolean, null, null },
        { "1", JsonSchemaTypes.Number, null, null },
        { "foo", JsonSchemaTypes.String, null, null },
        { "1,2", JsonSchemaTypes.Array, JsonSchemaTypes.Number, null },
        { "1,foo", JsonSchemaTypes.Array, JsonSchemaTypes.Number, "Parameter 'test-header' is not of type 'array[Number]'" },
    };

    [Theory]
    [MemberData(nameof(HeaderParameterTypeMismatchData))]
    public void Validate_ThrowsException_IfHeaderParameterIsNotOfSpecifiedType(
        string parameterValue,
        JsonSchemaType specifiedType,
        JsonSchemaType? specifiedItemsType,
        string? expectedErrorMessage)
    {
        var openApiDocument = DocumentWithOperation("/api/products", HttpMethod.Get, new OpenApiOperation
        {
            Parameters =
            [
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
            ]
        });
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri("/api/products", UriKind.Relative),
            Method = HttpMethod.Get
        };
        if (parameterValue != null) request.Headers.Add("test-header", parameterValue);

        var exception = Record.Exception(() =>
        {
            Subject().Validate(request, openApiDocument, "/api/products", HttpMethod.Get);
        });

        Assert.Equal(expectedErrorMessage, exception?.Message);
    }
#nullable restore

    [Theory]
    [InlineData(null, "Required content is not present")]
    [InlineData("foo", null)]
    public void Validate_ThrowsException_IfRequiredContentIsNotPresent(
        string contentString,
        string expectedErrorMessage)
    {
        var openApiDocument = DocumentWithOperation("/api/products", HttpMethod.Post, new OpenApiOperation
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
            Subject().Validate(request, openApiDocument, "/api/products", HttpMethod.Post);
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
        var openApiDocument = DocumentWithOperation("/api/products", HttpMethod.Post, new OpenApiOperation
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
            Subject().Validate(request, openApiDocument, "/api/products", HttpMethod.Post);
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
        var openApiDocument = DocumentWithOperation("/api/products", HttpMethod.Post, new OpenApiOperation
        {
            RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    [ "application/json" ] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = JsonSchemaTypes.Object,
                            Required = [ "prop1", "prop2" ],
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
            Subject([new JsonContentValidator()]).Validate(request, openApiDocument, "/api/products", HttpMethod.Post);
        });

        Assert.Equal(expectedErrorMessage, exception?.Message);
    }

    private static OpenApiDocument DocumentWithOperation(string pathTemplate, HttpMethod operationType, OpenApiOperation operationSpec)
    {
        return new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                [pathTemplate] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [operationType] = operationSpec
                    }
                }
            },
            Components = new OpenApiComponents
            {
                Schemas = []
            }
        };
    }

    private static RequestValidator Subject(IEnumerable<IContentValidator> contentValidators = null)
    {
        return new RequestValidator(contentValidators ?? []);
    }
}
