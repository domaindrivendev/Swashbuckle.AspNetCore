using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
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
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public partial class VerifyTests
{
    [Fact]
    public async Task ApiDescriptionsWithMatchingGroupName()
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource"),

                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "GET", relativePath: "resource"),

                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v2", httpMethod: "POST", relativePath: "resource"),
            ],
            options: new SwaggerGeneratorOptions
            {
                SwaggerDocs = new Dictionary<string, OpenApiInfo>
                {
                    ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                }
            }
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task ActionWithRouteNameMetadata()
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithRouteNameMetadata), groupName: "v1", httpMethod: "POST", relativePath: "resource"),
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task ActionWithEndpointNameMetadata()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithParameter));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = [new EndpointNameMetadata("SomeEndpointName")],
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create(actionDescriptor, methodInfo, groupName: "v1", httpMethod: "POST", relativePath: "resource"),
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task ActionWithProvidedOpenApiMetadata()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithParameter));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata =
            [
                new OpenApiOperation
                {
                    OperationId = "OperationIdSetInMetadata",
                    Parameters =
                    [
                        new OpenApiParameter
                        {
                            Name = "ParameterInMetadata"
                        }
                    ]
                }
            ],
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create(actionDescriptor, methodInfo, groupName: "v1", httpMethod: "POST", relativePath: "resource"),
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task ActionWithProducesAttributeAndProvidedOpenApiOperation()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithProducesAttribute));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata =
            [
                new OpenApiOperation
                {
                    OperationId = "OperationIdSetInMetadata",
                    Responses = new()
                    {
                        ["200"] = new OpenApiResponse()
                        {
                            Content = new Dictionary<string, OpenApiMediaType>()
                            {
                                ["application/someMediaType"] = new()
                            }
                        }
                    }
                }
            ],
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create(
                    actionDescriptor,
                    methodInfo,
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    supportedResponseTypes:
                    [
                        new ApiResponseType()
                        {
                            StatusCode = 200,
                            Type = typeof(TestDto)
                        }
                    ]),
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task ActionWithConsumesAttributeAndProvidedOpenApiOperation()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithConsumesAttribute));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata =
            [
                new OpenApiOperation
                {
                    OperationId = "OperationIdSetInMetadata",
                    RequestBody = new OpenApiRequestBody()
                    {
                        Content = new Dictionary<string, OpenApiMediaType>()
                        {
                            ["application/someMediaType"] = new()
                        }
                    }
                }
            ],
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create(
                    actionDescriptor,
                    methodInfo,
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions:
                    [
                        new ApiParameterDescription()
                        {
                            Name = "param",
                            Source = BindingSource.Body,
                            ModelMetadata = ModelMetadataFactory.CreateForType(typeof(TestDto))
                        }
                    ]),
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task ActionWithParameterAndProvidedOpenApiOperation()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithParameter));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata =
            [
                new OpenApiOperation
                {
                    OperationId = "OperationIdSetInMetadata",
                    Parameters =
                    [
                        new OpenApiParameter
                        {
                            Name = "ParameterInMetadata"
                        }
                    ]
                }
            ],
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create(
                    actionDescriptor,
                    methodInfo,
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions:
                    [
                        new ApiParameterDescription
                        {
                            Name = "ParameterInMetadata",
                            ModelMetadata = ModelMetadataFactory.CreateForType(typeof(string)),
                            Type = typeof(string)
                        }
                    ]),
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task ActionHasObsoleteAttribute()
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithObsoleteAttribute), groupName: "v1", httpMethod: "POST", relativePath: "resource"),
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task SortKeySelectorIsSpecified()
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource3"),

                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource1"),

                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource2"),
            ],
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

        await Verify(document);
    }

    [Fact]
    public async Task TagSelectorIsSpecified()
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource"),
            ],
            options: new SwaggerGeneratorOptions
            {
                SwaggerDocs = new Dictionary<string, OpenApiInfo>
                {
                    ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                },
                TagsSelector = (apiDesc) => [apiDesc.RelativePath]
            }
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task EndpointMetadataHasTags()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithParameter));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = [new TagsAttribute("Some", "Tags", "Here")],
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create(actionDescriptor, methodInfo, groupName: "v1", httpMethod: "POST", relativePath: "resource"),
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Theory]
    [InlineData(nameof(BindingSource.Query))]
    [InlineData(nameof(BindingSource.Header))]
    [InlineData(nameof(BindingSource.Path))]
    [InlineData(null)]
    public async Task ApiParametersThatAreNotBoundToBodyOrForm(string bindingSourceId)
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithParameter),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions:
                    [
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = (bindingSourceId != null) ? new BindingSource(bindingSourceId, null, false, true) : null
                        }
                    ])
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verifier.Verify(ToJson(document))
            .UseDirectory("snapshots")
            .UseParameters(bindingSourceId)
            .UniqueForTargetFrameworkAndVersion();
    }

    [Fact]
    public async Task OperationHasSwaggerIgnoreAttribute()
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithSwaggerIgnoreAttribute),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "ignored",
                    parameterDescriptions: []
                )
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task ActionParameterHasBindNeverAttribute()
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithParameterWithBindNeverAttribute),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions:
                    [
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = BindingSource.Query
                        }
                    ])
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task ActionParameterHasSwaggerIgnoreAttribute()
    {
        var subject = Subject(
            [
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithIntParameterWithSwaggerIgnoreAttribute),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions:
                    [
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = BindingSource.Query
                        }
                    ]
                )
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Theory]
    [InlineData(nameof(FakeController.ActionWithAcceptFromHeaderParameter))]
    [InlineData(nameof(FakeController.ActionWithContentTypeFromHeaderParameter))]
    [InlineData(nameof(FakeController.ActionWithAuthorizationFromHeaderParameter))]
    public async Task ActionsWithIllegalHeaderParameters(string action)
    {
        var illegalParameter = typeof(FakeController).GetMethod(action).GetParameters()[0];
        var fromHeaderAttribute = illegalParameter.GetCustomAttribute<FromHeaderAttribute>();

        var subject = Subject(
            [
                ApiDescriptionFactory.Create<FakeController>(
                    c => action,
                    groupName: "v1",
                    httpMethod: "GET",
                    relativePath: "resource",
                    parameterDescriptions:
                    [
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
                    ]
                )
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verifier.Verify(ToJson(document))
            .UseDirectory("snapshots")
            .UseParameters(action)
            .UniqueForTargetFrameworkAndVersion();
    }

    [Theory]
    [InlineData(nameof(FakeController.ActionWithAcceptFromHeaderParameter))]
    [InlineData(nameof(FakeController.ActionWithContentTypeFromHeaderParameter))]
    [InlineData(nameof(FakeController.ActionWithAuthorizationFromHeaderParameter))]
    public async Task ActionParameterIsIllegalHeaderParameterWithProvidedOpenApiOperation(string action)
    {
        var illegalParameter = typeof(FakeController).GetMethod(action).GetParameters()[0];
        var fromHeaderAttribute = illegalParameter.GetCustomAttribute<FromHeaderAttribute>();
        var illegalParameterName = fromHeaderAttribute?.Name ?? illegalParameter.Name;
        var methodInfo = typeof(FakeController).GetMethod(action);
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata =
            [
                new OpenApiOperation
                {
                    OperationId = "OperationIdSetInMetadata",
                    Parameters =
                    [
                        new OpenApiParameter
                        {
                            Name = illegalParameterName,
                        },
                        new OpenApiParameter
                        {
                            Name = "param",
                        }
                    ]
                }
            ],
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create(
                    actionDescriptor,
                    methodInfo,
                    groupName: "v1",
                    httpMethod: "GET",
                    relativePath: "resource",
                    parameterDescriptions:
                    [
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
                            ModelMetadata = ModelMetadataFactory.CreateForType(typeof(string)),
                            Type = typeof(string)
                        }
                    ]),
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verifier.Verify(ToJson(document))
            .UseDirectory("snapshots")
            .UseParameters(action)
            .UseMethodName("IllegalHeaderForOperation")
            .UniqueForTargetFrameworkAndVersion();
    }

    [Fact]
    public async Task ApiParameterIsBoundToPath()
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithParameter),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions:
                    [
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = BindingSource.Path
                        }
                    ])
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Theory]
    [InlineData(nameof(FakeController.ActionWithParameterWithRequiredAttribute))]
    [InlineData(nameof(FakeController.ActionWithParameterWithBindRequiredAttribute))]
    public async Task ActionWithRequiredQueryParameter(string action)
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create(
                    methodInfo: typeof(FakeController).GetMethod(action),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions:
                    [
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = BindingSource.Query
                        }
                    ])
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verifier.Verify(ToJson(document))
            .UseDirectory("snapshots")
            .UseParameters(action)
            .UniqueForTargetFrameworkAndVersion();
    }

    [Theory]
    [InlineData(nameof(FakeController.ActionWithParameterWithRequiredAttribute))]
    [InlineData(nameof(FakeController.ActionWithParameterWithBindRequiredAttribute))]
    public async Task ActionWithRequiredBodyParameter(string action)
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create(
                    methodInfo: typeof(FakeController).GetMethod(action),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions:
                    [
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = BindingSource.Body,
                        }
                    ],
                    supportedRequestFormats:
                    [
                        new ApiRequestFormat { MediaType = "application/json" }
                    ])
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verifier.Verify(ToJson(document))
            .UseDirectory("snapshots")
            .UseParameters(action)
            .UniqueForTargetFrameworkAndVersion();
    }

    [Fact]
    public async Task ActionWithRequiredMember()
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create(
                    methodInfo: typeof(FakeController).GetMethod(nameof(FakeController.ActionWithRequiredMember)),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions:
                    [
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = BindingSource.Query,
                            ModelMetadata = ModelMetadataFactory.CreateForProperty(typeof(FakeController.TypeWithRequiredProperty), "RequiredProperty")
                        }
                    ])
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task EndpointMetadataHasSummaryAttribute()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithParameter));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = [new EndpointSummaryAttribute("A Test Summary")],
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create(actionDescriptor, methodInfo, groupName: "v1", httpMethod: "POST", relativePath: "resource"),
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task EndpointMetadataHasDescriptionAttribute()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithParameter));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = [new EndpointDescriptionAttribute("A Test Description")],
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create(actionDescriptor, methodInfo, groupName: "v1", httpMethod: "POST", relativePath: "resource"),
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task ApiParameterDescriptionForBodyIsRequired()
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
            apiDescriptions:
            [
                ApiDescriptionFactory.Create(actionDescriptor, action.Method, groupName: "v1", httpMethod: "POST", relativePath: "resource", parameterDescriptions: [parameter]),
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task ApiParameterHasNoCorrespondingActionParameter()
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions:
                    [
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = BindingSource.Path
                        }
                    ])
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task ApiParametersThatAreBoundToForm()
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithMultipleParameters),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions:
                    [
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

                    ]
                )
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Theory]
    [InlineData("Body")]
    [InlineData("Form")]
    public async Task ActionHasConsumesAttribute(string bindingSourceId)
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithConsumesAttribute),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions:
                    [
                        new ApiParameterDescription
                        {
                            Name = "param",
                            Source = new BindingSource(bindingSourceId, null, false, true)
                        }
                    ])
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verifier.Verify(ToJson(document))
            .UseDirectory("snapshots")
            .UseParameters(bindingSourceId)
            .UniqueForTargetFrameworkAndVersion();
    }

    [Fact]
    public async Task ActionWithReturnValueAndSupportedResponseTypes()
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithReturnValue),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    supportedResponseTypes:
                    [
                        new ApiResponseType
                        {
                            ApiResponseFormats = [new ApiResponseFormat { MediaType = "application/json" }],
                            StatusCode = 200,
                        },
                        new ApiResponseType
                        {
                            ApiResponseFormats = [new ApiResponseFormat { MediaType = "application/json" }],
                            StatusCode = 400
                        },
                        new ApiResponseType
                        {
                            ApiResponseFormats = [new ApiResponseFormat { MediaType = "application/json" }],
                            StatusCode = 422
                        },
                        new ApiResponseType
                        {
                            ApiResponseFormats = [new ApiResponseFormat { MediaType = "application/json" }],
                            IsDefaultResponse = true
                        }

                    ]
                )
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task ActionHasFileResult()
    {
        var apiDescription = ApiDescriptionFactory.Create<FakeController>(
            c => nameof(c.ActionWithFileResult),
            groupName: "v1",
            httpMethod: "POST",
            relativePath: "resource",
            supportedResponseTypes:
            [
                new ApiResponseType
                {
                    ApiResponseFormats = [new ApiResponseFormat { MediaType = "application/zip" }],
                    StatusCode = 200,
                    Type = typeof(FileContentResult)
                }
            ]);

        // ASP.NET Core sets ModelMetadata to null for FileResults
        apiDescription.SupportedResponseTypes[0].ModelMetadata = null;

        var subject = Subject(
            apiDescriptions: [apiDescription]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task ActionHasProducesAttribute()
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithProducesAttribute),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    supportedResponseTypes:
                    [
                        new ApiResponseType
                        {
                            ApiResponseFormats = [new ApiResponseFormat { MediaType = "application/json" }],
                            StatusCode = 200,
                        }
                    ])
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task ConflictingActionsResolverIsSpecified()
    {
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource"),

                ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource")
            ],
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

        await Verify(document);
    }

    [Fact]
    public async Task ActionHavingFromFormAttributeButNotWithIFormFile()
    {
        var parameterInfo = typeof(FakeController)
            .GetMethod(nameof(FakeController.ActionHavingFromFormAttributeButNotWithIFormFile))
            .GetParameters()[0];

        var fileUploadParameterInfo = typeof(FakeController)
            .GetMethod(nameof(FakeController.ActionHavingFromFormAttributeButNotWithIFormFile))
            .GetParameters()[1];

        var subject = Subject(
            apiDescriptions:
            [
               ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionHavingFromFormAttributeButNotWithIFormFile),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions:
                    [
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
                    ])
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task ActionHavingFromFormAttributeWithSwaggerIgnore()
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
            apiDescriptions:
            [
               ApiDescriptionFactory.Create<FakeController>(
                    c => nameof(c.ActionHavingFromFormAttributeWithSwaggerIgnore),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions:
                    [
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
                    ])
            ]
        );
        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task GetSwagger_Works_As_Expected_When_FromFormObject()
    {
        var subject = Subject(
            apiDescriptions:
            [
               ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionHavingFromFormAttributeWithSwaggerIgnore),
                        groupName: "v1",
                        httpMethod: "POST",
                        relativePath: "resource",
                        parameterDescriptions:
                        [
                            new ApiParameterDescription
                            {
                                Name = "param1",
                                Source = BindingSource.Form,
                                Type = typeof(SwaggerIngoreAnnotatedType),
                                ModelMetadata = ModelMetadataFactory.CreateForType(typeof(SwaggerIngoreAnnotatedType))
                            }
                        ])
            ]
        );
        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task GetSwagger_Works_As_Expected_When_FromFormObject_AndString()
    {
        var subject = Subject(
            apiDescriptions:
            [
               ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionHavingFromFormObjectAndString),
                        groupName: "v1",
                        httpMethod: "POST",
                        relativePath: "resource",
                        parameterDescriptions:
                        [
                            new ApiParameterDescription
                            {
                                Name = "param1",
                                Source = BindingSource.Form,
                                Type = typeof(SwaggerIngoreAnnotatedType),
                                ModelMetadata = ModelMetadataFactory.CreateForType(typeof(SwaggerIngoreAnnotatedType))
                            },
                            new ApiParameterDescription
                            {
                                Name = "param2",
                                Source = BindingSource.Form,
                                Type = typeof(string),
                                ModelMetadata = ModelMetadataFactory.CreateForType(typeof(string))
                            }
                        ])
            ]
        );
        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task GetSwagger_Works_As_Expected_When_TypeIsEnum_AndModelMetadataTypeIsString()
    {
        var subject = Subject(
            apiDescriptions:
            [
               ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionHavingEnum),
                        groupName: "v1",
                        httpMethod: "POST",
                        relativePath: "resource",
                        parameterDescriptions:
                        [
                            new ApiParameterDescription
                            {
                                Name = "param1",
                                Source = BindingSource.Query,
                                Type = typeof(IntEnum),
                                ModelMetadata = ModelMetadataFactory.CreateForType(typeof(string))
                            }
                        ])
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task GetSwagger_Copies_Description_From_GeneratedSchema()
    {
        var propertyEnum = typeof(TypeWithDefaultAttributeOnEnum).GetProperty(nameof(TypeWithDefaultAttributeOnEnum.EnumWithDefault));
        var modelMetadataForEnum = new DefaultModelMetadata(
                                new DefaultModelMetadataProvider(new FakeICompositeMetadataDetailsProvider()),
                                new FakeICompositeMetadataDetailsProvider(),
                                new DefaultMetadataDetails(ModelMetadataIdentity.ForProperty(propertyEnum, typeof(IntEnum), typeof(TypeWithDefaultAttributeOnEnum)), ModelAttributes.GetAttributesForProperty(typeof(TypeWithDefaultAttributeOnEnum), propertyEnum)));

        var propertyEnumArray = typeof(TypeWithDefaultAttributeOnEnum).GetProperty(nameof(TypeWithDefaultAttributeOnEnum.EnumArrayWithDefault));
        var modelMetadataForEnumArray = new DefaultModelMetadata(
                                new DefaultModelMetadataProvider(new FakeICompositeMetadataDetailsProvider()),
                                new FakeICompositeMetadataDetailsProvider(),
                                new DefaultMetadataDetails(ModelMetadataIdentity.ForProperty(propertyEnumArray, typeof(IntEnum[]), typeof(TypeWithDefaultAttributeOnEnum)), ModelAttributes.GetAttributesForProperty(typeof(TypeWithDefaultAttributeOnEnum), propertyEnumArray)));
        var subject = Subject(
           apiDescriptions:
           [
               ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionHavingFromFormAttributeWithSwaggerIgnore),
                        groupName: "v1",
                        httpMethod: "POST",
                        relativePath: "resource",
                        parameterDescriptions:
                        [
                            new ApiParameterDescription
                            {
                                Name = nameof(TypeWithDefaultAttributeOnEnum.EnumWithDefault),
                                Source = BindingSource.Query,
                                Type = typeof(IntEnum),
                                ModelMetadata = modelMetadataForEnum
                            },
                            new ApiParameterDescription
                            {
                                Name = nameof(TypeWithDefaultAttributeOnEnum.EnumArrayWithDefault),
                                Source = BindingSource.Query,
                                Type = typeof(IntEnum[]),
                                ModelMetadata = modelMetadataForEnumArray
                            }
                        ])
           ],
           schemaFilters: [new TestEnumSchemaFilter()]
       );
        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task GetSwagger_GenerateConsumesSchemas_ForProvidedOpenApiOperationWithSeveralFromForms()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithConsumesAttribute));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata =
                [
                    new OpenApiOperation
                    {
                        OperationId = "OperationIdSetInMetadata",
                        RequestBody = new OpenApiRequestBody()
                        {
                            Content = new Dictionary<string, OpenApiMediaType>()
                            {
                                ["application/someMediaType"] = new()
                            }
                        }
                    }
                ],
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions:
            [
                    ApiDescriptionFactory.Create(
                        actionDescriptor,
                        methodInfo,
                        groupName: "v1",
                        httpMethod: "POST",
                        relativePath: "resource",
                        parameterDescriptions:
                        [
                            new ApiParameterDescription()
                            {
                                Name = "param",
                                Source = BindingSource.Form,
                                ModelMetadata = ModelMetadataFactory.CreateForType(typeof(TestDto))
                            },
                            new ApiParameterDescription()
                            {
                                Name = "param2",
                                Source = BindingSource.Form,
                                ModelMetadata = ModelMetadataFactory.CreateForType(typeof(TypeWithDefaultAttributeOnEnum))
                            }
                        ]),
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task GetSwagger_GenerateConsumesSchemas_ForProvidedOpenApiOperationWithIFormFile()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithConsumesAttribute));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata =
            [
                new OpenApiOperation
                    {
                        OperationId = "OperationIdSetInMetadata",
                        RequestBody = new OpenApiRequestBody()
                        {
                            Content = new Dictionary<string, OpenApiMediaType>()
                            {
                                ["application/someMediaType"] = new()
                            }
                        }
                    }
            ],
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create(
                        actionDescriptor,
                        methodInfo,
                        groupName: "v1",
                        httpMethod: "POST",
                        relativePath: "resource",
                        parameterDescriptions:
                        [
                            new ApiParameterDescription()
                            {
                                Name = "param",
                                Source = BindingSource.Form,
                                ModelMetadata = ModelMetadataFactory.CreateForType(typeof(IFormFile))
                            }
                        ]),
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task GetSwagger_GenerateConsumesSchemas_ForProvidedOpenApiOperationWithIFormFileCollection()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithConsumesAttribute));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata =
            [
                new OpenApiOperation
                    {
                        OperationId = "OperationIdSetInMetadata",
                        RequestBody = new OpenApiRequestBody()
                        {
                            Content = new Dictionary<string, OpenApiMediaType>()
                            {
                                ["application/someMediaType"] = new()
                            }
                        }
                    }
            ],
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create(
                        actionDescriptor,
                        methodInfo,
                        groupName: "v1",
                        httpMethod: "POST",
                        relativePath: "resource",
                        parameterDescriptions:
                        [
                            new ApiParameterDescription()
                            {
                                Name = "param",
                                Source = BindingSource.Form,
                                ModelMetadata = ModelMetadataFactory.CreateForType(typeof(IFormFileCollection))
                            }
                        ]),
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    [Fact]
    public async Task GetSwagger_GenerateConsumesSchemas_ForProvidedOpenApiOperationWithStringFromForm()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithConsumesAttribute));
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata =
            [
                new OpenApiOperation
                    {
                        OperationId = "OperationIdSetInMetadata",
                        RequestBody = new OpenApiRequestBody()
                        {
                            Content = new Dictionary<string, OpenApiMediaType>()
                            {
                                ["application/someMediaType"] = new()
                            }
                        }
                    }
            ],
            RouteValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            }
        };
        var subject = Subject(
            apiDescriptions:
            [
                ApiDescriptionFactory.Create(
                        actionDescriptor,
                        methodInfo,
                        groupName: "v1",
                        httpMethod: "POST",
                        relativePath: "resource",
                        parameterDescriptions:
                        [
                            new ApiParameterDescription()
                            {
                                Name = "param",
                                Source = BindingSource.Form,
                                ModelMetadata = ModelMetadataFactory.CreateForType(typeof(string))
                            }
                        ]),
            ]
        );

        var document = subject.GetSwagger("v1");

        await Verify(document);
    }

    private static SwaggerGenerator Subject(
            IEnumerable<ApiDescription> apiDescriptions,
            SwaggerGeneratorOptions options = null,
            IEnumerable<AuthenticationScheme> authenticationSchemes = null,
            List<ISchemaFilter> schemaFilters = null)
    {
        return new SwaggerGenerator(
            options ?? DefaultOptions,
            new FakeApiDescriptionGroupCollectionProvider(apiDescriptions),
            new SchemaGenerator(new SchemaGeneratorOptions() { SchemaFilters = schemaFilters ?? [] }, new JsonSerializerDataContractResolver(new JsonSerializerOptions())),
            new FakeAuthenticationSchemeProvider(authenticationSchemes ?? [])
        );
    }

    private static readonly SwaggerGeneratorOptions DefaultOptions = new()
    {
        SwaggerDocs = new Dictionary<string, OpenApiInfo>
        {
            ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
        }
    };

    private static string ToJson(OpenApiDocument document)
    {
        using var stringWriter = new StringWriter();
        var jsonWriter = new OpenApiJsonWriter(stringWriter);

        document.SerializeAsV3(jsonWriter);

        return NormalizeLineBreaks(stringWriter.ToString());
    }

    private static async Task Verify(OpenApiDocument document)
    {
        await Verifier.Verify(ToJson(document))
            .UseDirectory("snapshots")
            .UniqueForTargetFrameworkAndVersion();
    }

    private static string NormalizeLineBreaks(string swagger)
        => UnixNewLineRegex().Replace(swagger, "\\r\\n");

    [GeneratedRegex(@"(?<!\\r)\\n")]
    private static partial Regex UnixNewLineRegex();
}
