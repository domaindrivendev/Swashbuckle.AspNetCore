using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;
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

    [Fact]
    public Task SortKeySelectorIsSpecified()
    {
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource3"),

                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource1"),

                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource2"),
            },
            options: new SwaggerGeneratorOptions
            {
                SwaggerDocs = new Dictionary<string, OpenApiInfo>
                {
                    ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                },
                SortKeySelector = (apiDesc) => apiDesc.RelativePath
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task TagSelectorIsSpecified()
    {
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource"),
            },
            options: new SwaggerGeneratorOptions
            {
                SwaggerDocs = new Dictionary<string, OpenApiInfo>
                {
                    ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                },
                TagsSelector = (apiDesc) => new[] { apiDesc.RelativePath }
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task EndpointMetadataHasTags()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithParameter));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = new List<object>() { new TagsAttribute("Some", "Tags", "Here") },
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

        return Verifier.Verify(document)
            .UseParameters(action)
            .UseMethodName("IllegalHeaderParameterWithOpenApiOperation");
    }

    [Fact]
    public Task ApiParameterIsBoundToPath()
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
                            Source = BindingSource.Path
                        }
                    })
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Theory]
    [InlineData(nameof(FakeController.ActionWithParameterWithRequiredAttribute))]
    [InlineData(nameof(FakeController.ActionWithParameterWithBindRequiredAttribute))]
    public Task ActionWithRequiredQueryParameter(string action)
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

    [Theory]
    [InlineData(nameof(FakeController.ActionWithParameterWithRequiredAttribute))]
    [InlineData(nameof(FakeController.ActionWithParameterWithBindRequiredAttribute))]
    public Task ActionWithRequiredBodyParameter(string action)
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
                            Source = BindingSource.Body,
                        }
                    },
                    supportedRequestFormats: new[]
                    {
                        new ApiRequestFormat { MediaType = "application/json" }
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

    [Fact]
    public Task EndpointMetadataHasSummaryAttribute()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithParameter));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = new List<object>() { new EndpointSummaryAttribute("A Test Summary") },
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
    public Task EndpointMetadataHasDescriptionAttribute()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithParameter));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = new List<object>() { new EndpointDescriptionAttribute("A Test Description") },
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
#endif

    [Fact]
    public Task ApiParameterDescriptionForBodyIsRequired()
    {
        static void Execute(object obj) { }

        Action<object> action = Execute;

        var actionDescriptor = new ActionDescriptor
        {
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = "Foo",
            }
        };

        var parameter = new ApiParameterDescription
        {
            Name = "obj",
            Source = BindingSource.Body,
            IsRequired = true,
            Type = typeof(object),
            ModelMetadata = ModelMetadataFactory.CreateForParameter(action.Method.GetParameters()[0])
        };

        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create(actionDescriptor, action.Method, groupName: "v1", httpMethod: "POST", relativePath: "resource", parameterDescriptions: new[]{ parameter }),
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task ApiParameterHasNoCorrespondingActionParameter()
    {
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions: new []
                    {
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = BindingSource.Path
                        }
                    })
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task ApiParametersThatAreBoundToForm()
    {
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithMultipleParameters),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions: new []
                    {
                        new ApiParameterDescription
                        {
                            Name = "param1",
                            Source = BindingSource.Form,
                        },
                        new ApiParameterDescription
                        {
                            Name = "param2",
                            Source = BindingSource.Form,
                        }

                    }
                )
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Theory]
    [InlineData("Body")]
    [InlineData("Form")]
    public Task ActionHasConsumesAttribute(string bindingSourceId)
    {
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithConsumesAttribute),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions: new []
                    {
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = new BindingSource(bindingSourceId, null, false, true)
                        }
                    })
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document).UseParameters(bindingSourceId);
    }

    [Fact]
    public Task ActionWithReturnValueAndSupportedResponseTypes()
    {
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithReturnValue),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    supportedResponseTypes: new []
                    {
                        new ApiResponseType
                        {
                            ApiResponseFormats = new [] { new ApiResponseFormat { MediaType = "application/json" } },
                            StatusCode = 200,
                        },
                        new ApiResponseType
                        {
                            ApiResponseFormats = new [] { new ApiResponseFormat { MediaType = "application/json" } },
                            StatusCode = 400
                        },
                        new ApiResponseType
                        {
                            ApiResponseFormats = new [] { new ApiResponseFormat { MediaType = "application/json" } },
                            StatusCode = 422
                        },
                        new ApiResponseType
                        {
                            ApiResponseFormats = new [] { new ApiResponseFormat { MediaType = "application/json" } },
                            IsDefaultResponse = true
                        }

                    }
                )
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task ActionHasFileResult()
    {
        var apiDescription = ApiDescriptionFactory.Create<FakeController>(
            c => nameof(c.ActionWithFileResult),
            groupName: "v1",
            httpMethod: "POST",
            relativePath: "resource",
            supportedResponseTypes: new[]
            {
                new ApiResponseType
                {
                    ApiResponseFormats = new [] { new ApiResponseFormat { MediaType = "application/zip" } },
                    StatusCode = 200,
                    Type = typeof(FileContentResult)
                }
            });

        // ASP.NET Core sets ModelMetadata to null for FileResults
        apiDescription.SupportedResponseTypes[0].ModelMetadata = null;

        var subject = Subject(
            apiDescriptions: new[] { apiDescription }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task ActionHasProducesAttribute()
    {
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithProducesAttribute),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    supportedResponseTypes: new []
                    {
                        new ApiResponseType
                        {
                            ApiResponseFormats = new [] { new ApiResponseFormat { MediaType = "application/json" } },
                            StatusCode = 200,
                        }
                    })
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task ConflictingActionsResolverIsSpecified()
    {
        var subject = Subject(
            apiDescriptions: new[]
            {
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource"),

                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource")
            },
            options: new SwaggerGeneratorOptions
            {
                SwaggerDocs = new Dictionary<string, OpenApiInfo>
                {
                    ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                },
                ConflictingActionsResolver = (apiDescriptions) => apiDescriptions.First()
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task ActionHavingFromFormAttributeButNotWithIFormFile()
    {
        var parameterInfo = typeof(FakeController)
            .GetMethod(nameof(FakeController.ActionHavingFromFormAttributeButNotWithIFormFile))
            .GetParameters()[0];

        var fileUploadParameterInfo = typeof(FakeController)
            .GetMethod(nameof(FakeController.ActionHavingFromFormAttributeButNotWithIFormFile))
            .GetParameters()[1];

        var subject = Subject(
            apiDescriptions: new[]
            {
               ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionHavingFromFormAttributeButNotWithIFormFile),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions: new[]
                    {
                        new ApiParameterDescription
                        {
                            Name = "param1", // Name of the parameter
                            Type = typeof(string), // Type of the parameter
                            ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo }
                        },
                        new ApiParameterDescription
                        {
                            Name = "param2", // Name of the parameter
                            Type = typeof(IFormFile), // Type of the parameter
                            ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = fileUploadParameterInfo }
                        }
                    })
            }
        );

        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

    [Fact]
    public Task ActionHavingFromFormAttributeWithSwaggerIgnore()
    {
        var propertyIgnored = typeof(SwaggerIngoreAnnotatedType).GetProperty(nameof(SwaggerIngoreAnnotatedType.IgnoredString));
        var modelMetadataIgnored = new DefaultModelMetadata(
                                new DefaultModelMetadataProvider(new FakeICompositeMetadataDetailsProvider()),
                                new FakeICompositeMetadataDetailsProvider(),
                                new DefaultMetadataDetails(ModelMetadataIdentity.ForProperty(propertyIgnored, typeof(string), typeof(SwaggerIngoreAnnotatedType)), ModelAttributes.GetAttributesForProperty(typeof(SwaggerIngoreAnnotatedType), propertyIgnored)));

        var propertyNotIgnored = typeof(SwaggerIngoreAnnotatedType).GetProperty(nameof(SwaggerIngoreAnnotatedType.NotIgnoredString));
        var modelMetadataNotIgnored = new DefaultModelMetadata(
                                new DefaultModelMetadataProvider(new FakeICompositeMetadataDetailsProvider()),
                                new FakeICompositeMetadataDetailsProvider(),
                                new DefaultMetadataDetails(ModelMetadataIdentity.ForProperty(propertyNotIgnored, typeof(string), typeof(SwaggerIngoreAnnotatedType)), ModelAttributes.GetAttributesForProperty(typeof(SwaggerIngoreAnnotatedType), propertyNotIgnored)));
        var subject = Subject(
            apiDescriptions: new[]
            {
               ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionHavingFromFormAttributeWithSwaggerIgnore),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions: new[]
                    {
                        new ApiParameterDescription
                        {
                            Name = nameof(SwaggerIngoreAnnotatedType.IgnoredString),
                            Source = BindingSource.Form,
                            Type = typeof(string),
                            ModelMetadata = modelMetadataIgnored
                        },
                        new ApiParameterDescription
                        {
                            Name = nameof(SwaggerIngoreAnnotatedType.NotIgnoredString),
                            Source = BindingSource.Form,
                            Type = typeof(string),
                            ModelMetadata = modelMetadataNotIgnored
                        }
                    })
            }
        );
        var document = subject.GetSwagger("v1");

        return Verifier.Verify(document);
    }

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
