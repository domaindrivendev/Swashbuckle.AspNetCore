using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.TestSupport;
using VerifyXunit;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class SwaggerGeneratorVerifyTests
{
    [Fact]
    public Task ApiDescriptionsWithMatchingGroupName()
    {
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource"),

                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "GET", relativePath: "resource"),

                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v2", httpMethod: "POST", relativePath: "resource"),
            },
            options: new SwaggerGeneratorOptions
            {
                SwaggerDocs = new Dictionary<string, OpenApiInfo>
                {
                    ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                }
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task ActionWithRouteNameMetadata()
    {
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithRouteNameMetadata), groupName: "v1", httpMethod: "POST", relativePath: "resource"),
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task ActionWithEndpointNameMetadata()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithParameter));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = new List<object>() { new EndpointNameMetadata("SomeEndpointName") },
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create(actionDescriptor, methodInfo, groupName: "v1", httpMethod: "POST", relativePath: "resource"),
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task ActionWithProvidedOpenApiMetadata()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithParameter));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = new List<object>()
            {
                new OpenApiOperation
                {
                    OperationId = "OperationIdSetInMetadata",
                    Parameters = new List<OpenApiParameter>()
                    {
                        new OpenApiParameter
                        {
                            Name = "ParameterInMetadata"
                        }
                    }
                }
            },
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create(actionDescriptor, methodInfo, groupName: "v1", httpMethod: "POST", relativePath: "resource"),
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task ActionWithProducesAttributeAndProvidedOpenApiOperation()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithProducesAttribute));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = new List<object>()
            {
                new OpenApiOperation
                {
                    OperationId = "OperationIdSetInMetadata",
                    Responses = new()
                    {
                        ["200"] = new()
                        {
                            Content = new Dictionary<string, OpenApiMediaType>()
                            {
                                ["application/someMediaType"] = new()
                            }
                        }
                    }
                }
            },
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create(
                    actionDescriptor,
                    methodInfo,
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    supportedResponseTypes: new[]
                    {
                        new ApiResponseType()
                        {
                            StatusCode = 200,
                            Type = typeof(TestDto)
                        }
                    }),
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task ActionWithConsumesAttributeAndProvidedOpenApiOperation()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithConsumesAttribute));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = new List<object>()
            {
                new OpenApiOperation
                {
                    OperationId = "OperationIdSetInMetadata",
                    RequestBody = new()
                    {
                        Content = new Dictionary<string, OpenApiMediaType>()
                        {
                            ["application/someMediaType"] = new()
                        }
                    }
                }
            },
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create(
                    actionDescriptor,
                    methodInfo,
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions: new[]
                    {
                        new ApiParameterDescription()
                        {
                            Name = "param",
                            Source = BindingSource.Body,
                            ModelMetadata = ModelMetadataFactory.CreateForType(typeof(TestDto))
                        }
                    }),
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task ActionWithParameterAndProvidedOpenApiOperation()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithParameter));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = new List<object>()
            {
                new OpenApiOperation
                {
                    OperationId = "OperationIdSetInMetadata",
                    Parameters = new List<OpenApiParameter>()
                    {
                        new OpenApiParameter
                        {
                            Name = "ParameterInMetadata"
                        }
                    }
                }
            },
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create(
                    actionDescriptor,
                    methodInfo,
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions: new[]
                    {
                        new ApiParameterDescription
                        {
                            Name = "ParameterInMetadata",
                            ModelMetadata = ModelMetadataFactory.CreateForType(typeof(string))
                        }
                    }),
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task ActionHasObsoleteAttribute()
    {
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithObsoleteAttribute), groupName: "v1", httpMethod: "POST", relativePath: "resource"),
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Theory]
    [InlineData(nameof(BindingSource.Query))]
    [InlineData(nameof(BindingSource.Header))]
    [InlineData(nameof(BindingSource.Path))]
    [InlineData(null)]
    public Task ApiParametersThatAreNotBoundToBodyOrForm(string bindingSourceId)
    {
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithParameter),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions: new []
                    {
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = (bindingSourceId != null) ? new BindingSource(bindingSourceId, null, false, true) : null
                        }
                    })
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document).UseParameters(bindingSourceId);
    }

    [Fact]
    public Task OperationHasSwaggerIgnoreAttribute()
    {
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithSwaggerIgnoreAttribute),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "ignored",
                    parameterDescriptions: Array.Empty<ApiParameterDescription>()
                )
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task ActionParameterHasBindNeverAttribute()
    {
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithParameterWithBindNeverAttribute),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions: new []
                    {
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = BindingSource.Query
                        }
                    })
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task ActionParameterHasSwaggerIgnoreAttribute()
    {
        var subject = Subject(
            new[]
            {
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithIntParameterWithSwaggerIgnoreAttribute),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions: new[]
                    {
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = BindingSource.Query
                        }
                    }
                )
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Theory]
    [InlineData(nameof(FakeController.ActionWithAcceptFromHeaderParameter))]
    [InlineData(nameof(FakeController.ActionWithContentTypeFromHeaderParameter))]
    [InlineData(nameof(FakeController.ActionWithAuthorizationFromHeaderParameter))]
    public Task ActionsWithIllegalHeaderParameters(string action)
    {
        var illegalParameter = typeof(FakeController).GetMethod(action).GetParameters()[0];
        var fromHeaderAttribute = illegalParameter.GetCustomAttribute<FromHeaderAttribute>();

        var subject = Subject(
            new[]
            {
                ApiDescriptionFactory.Create<FakeController>(
                    c => action,
                    groupName: "v1",
                    httpMethod: "GET",
                    relativePath: "resource",
                    parameterDescriptions: new[]
                    {
                        new ApiParameterDescription
                        {
                            Name = fromHeaderAttribute?.Name ?? illegalParameter.Name,
                            Source = BindingSource.Header,
                            ModelMetadata = ModelMetadataFactory.CreateForParameter(illegalParameter)
                        },
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = BindingSource.Header
                        }
                    }
                )
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document).UseParameters(action);
    }

    [Theory]
    [InlineData(nameof(FakeController.ActionWithAcceptFromHeaderParameter))]
    [InlineData(nameof(FakeController.ActionWithContentTypeFromHeaderParameter))]
    [InlineData(nameof(FakeController.ActionWithAuthorizationFromHeaderParameter))]
    public Task ActionParameterIsIllegalHeaderParameterWithProvidedOpenApiOperation(string action)
    {
        var illegalParameter = typeof(FakeController).GetMethod(action).GetParameters()[0];
        var fromHeaderAttribute = illegalParameter.GetCustomAttribute<FromHeaderAttribute>();
        var illegalParameterName = fromHeaderAttribute?.Name ?? illegalParameter.Name;
        var methodInfo = typeof(FakeController).GetMethod(action);
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = new List<object>()
            {
                new OpenApiOperation
                {
                    OperationId = "OperationIdSetInMetadata",
                    Parameters = new List<OpenApiParameter>()
                    {
                        new OpenApiParameter
                        {
                            Name = illegalParameterName,
                        },
                        new OpenApiParameter
                        {
                            Name = "param",
                        }
                    }
                }
            },
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create(
                    actionDescriptor,
                    methodInfo,
                    groupName: "v1",
                    httpMethod: "GET",
                    relativePath: "resource",
                    parameterDescriptions: new[]
                    {
                        new ApiParameterDescription
                        {
                            Name = illegalParameterName,
                            Source = BindingSource.Header,
                            ModelMetadata = ModelMetadataFactory.CreateForParameter(illegalParameter)
                        },
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = BindingSource.Header,
                            ModelMetadata = ModelMetadataFactory.CreateForType(typeof(string))
                        }
                    }),
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document).UseParameters(action);
    }

    [Theory]
    [InlineData(nameof(FakeController.ActionWithParameterWithRequiredAttribute))]
    [InlineData(nameof(FakeController.ActionWithParameterWithBindRequiredAttribute))]
    public Task ActionWithRequiredParameter(string action)
    {
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create(
                    methodInfo: typeof(FakeController).GetMethod(action),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions: new []
                    {
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = BindingSource.Query
                        }
                    })
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document).UseParameters(action);
    }

#if NET7_0_OR_GREATER
    [Fact]
    public Task ActionWithRequiredMember()
    {
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create(
                    methodInfo: typeof(FakeController).GetMethod(nameof(FakeController.ActionWithRequiredMember)),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions: new []
                    {
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = BindingSource.Query,
                            ModelMetadata = ModelMetadataFactory.CreateForProperty(typeof(FakeController.TypeWithRequiredProperty), "RequiredProperty")
                        }
                    })
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }
#endif

    private static SwaggerGenerator Subject(
        IEnumerable<ApiDescription> apiDescriptions,
        SwaggerGeneratorOptions options = null,
        IEnumerable<AuthenticationScheme> authenticationSchemes = null)
    {
        return new SwaggerGenerator(
            options ?? DefaultOptions,
            new FakeApiDescriptionGroupCollectionProvider(apiDescriptions),
            new SchemaGenerator(new SchemaGeneratorOptions(), new JsonSerializerDataContractResolver(new JsonSerializerOptions())),
            new FakeAuthenticationSchemeProvider(authenticationSchemes ?? Enumerable.Empty<AuthenticationScheme>())
        );
    }

    private static readonly SwaggerGeneratorOptions DefaultOptions = new SwaggerGeneratorOptions
    {
        SwaggerDocs = new Dictionary<string, OpenApiInfo>
        {
            ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
        }
    };
}
