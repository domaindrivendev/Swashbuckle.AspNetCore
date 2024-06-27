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
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;
using Swashbuckle.AspNetCore.TestSupport;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class SwaggerGeneratorTests
    {
        [Fact]
        public void GetSwagger_GeneratesSwaggerDocument_ForApiDescriptionsWithMatchingGroupName()
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

            Assert.Equal("V1", document.Info.Version);
            Assert.Equal("Test API", document.Info.Title);
            Assert.Equal(new[] { "/resource" }, document.Paths.Keys.ToArray());
            Assert.Equal(new[] { OperationType.Post, OperationType.Get }, document.Paths["/resource"].Operations.Keys);
        }

        [Theory]
        [InlineData("resources/{id}", "/resources/{id}")]
        [InlineData("resources;secondary={secondary}", "/resources;secondary={secondary}")]
        [InlineData("resources:deposit", "/resources:deposit")]
        [InlineData("{category}/{product?}/{sku}", "/{category}/{product}/{sku}")]
        [InlineData("{area=Home}/{controller:required}/{id=0:int}", "/{area}/{controller}/{id}")]
        [InlineData("{category}/product/{group?}", "/{category}/product/{group}")]
        [InlineData("{category:int}/product/{group:range(10, 20)?}", "/{category}/product/{group}")]
        [InlineData("{person:int}/{ssn:regex(^\\d{{3}}-\\d{{2}}-\\d{{4}}$)}", "/{person}/{ssn}")]
        [InlineData("{person:int}/{ssn:regex(^(?=.*kind)(?=.*good).*$)}", "/{person}/{ssn}")]
        public void GetSwagger_GeneratesSwaggerDocument_ForApiDescriptionsWithConstrainedRelativePaths(string path, string expectedPath)
        {
            var subject = Subject(
                apiDescriptions: new[]
                {
                    ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: path),

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

            Assert.Equal("V1", document.Info.Version);
            Assert.Equal("Test API", document.Info.Title);
            var (actualPath, _) = Assert.Single(document.Paths);
            Assert.Equal(expectedPath, actualPath);
        }

        [Fact]
        public void GetSwagger_SetsOperationIdToNull_ByDefault()
        {
            var subject = Subject(
                apiDescriptions: new[]
                {
                    ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource"),
                }
            );

            var document = subject.GetSwagger("v1");

            Assert.Null(document.Paths["/resource"].Operations[OperationType.Post].OperationId);
        }

        [Fact]
        public void GetSwagger_SetsOperationIdToRouteName_IfActionHasRouteNameMetadata()
        {
            var subject = Subject(
                apiDescriptions: new[]
                {
                    ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionWithRouteNameMetadata), groupName: "v1", httpMethod: "POST", relativePath: "resource"),
                }
            );

            var document = subject.GetSwagger("v1");

            Assert.Equal("SomeRouteName", document.Paths["/resource"].Operations[OperationType.Post].OperationId);
        }

        [Fact]
        public void GetSwagger_SetsOperationIdToEndpointName_IfActionHasEndpointNameMetadata()
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

            Assert.Equal("SomeEndpointName", document.Paths["/resource"].Operations[OperationType.Post].OperationId);
        }

        [Fact]
        public void GetSwagger_UseProvidedOpenApiOperation_IfExistsInMetadata()
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

            Assert.Equal("OperationIdSetInMetadata", document.Paths["/resource"].Operations[OperationType.Post].OperationId);
            Assert.Equal("ParameterInMetadata", document.Paths["/resource"].Operations[OperationType.Post].Parameters[0].Name);
        }

        [Fact]
        public void GetSwagger_GenerateProducesSchemas_ForProvidedOpenApiOperation()
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

            Assert.Equal("OperationIdSetInMetadata", document.Paths["/resource"].Operations[OperationType.Post].OperationId);
            var content = Assert.Single(document.Paths["/resource"].Operations[OperationType.Post].Responses["200"].Content);
            Assert.Equal("application/someMediaType", content.Key);
            Assert.Null(content.Value.Schema.Type);
            Assert.NotNull(content.Value.Schema.Reference);
            Assert.Equal("TestDto", content.Value.Schema.Reference.Id);
        }

        [Fact]
        public void GetSwagger_GenerateConsumesSchemas_ForProvidedOpenApiOperation()
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

            Assert.Equal("OperationIdSetInMetadata", document.Paths["/resource"].Operations[OperationType.Post].OperationId);
            var content = Assert.Single(document.Paths["/resource"].Operations[OperationType.Post].RequestBody.Content);
            Assert.Equal("application/someMediaType", content.Key);
            Assert.Null(content.Value.Schema.Type);
            Assert.NotNull(content.Value.Schema.Reference);
            Assert.Equal("TestDto", content.Value.Schema.Reference.Id);
        }

        [Fact]
        public void GetSwagger_GenerateParametersSchemas_ForProvidedOpenApiOperation()
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

            Assert.Equal("OperationIdSetInMetadata", document.Paths["/resource"].Operations[OperationType.Post].OperationId);
            Assert.Equal("ParameterInMetadata", document.Paths["/resource"].Operations[OperationType.Post].Parameters[0].Name);
            Assert.NotNull(document.Paths["/resource"].Operations[OperationType.Post].Parameters[0].Schema);
            Assert.Equal("string", document.Paths["/resource"].Operations[OperationType.Post].Parameters[0].Schema.Type);
        }

        [Fact]
        public void GetSwagger_SetsOperationIdToNull_IfActionHasNoEndpointMetadata()
        {
            var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithParameter));
            var actionDescriptor = new ActionDescriptor
            {
                EndpointMetadata = null,
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

            Assert.Null(document.Paths["/resource"].Operations[OperationType.Post].OperationId);
        }

        [Fact]
        public void GetSwagger_SetsDeprecated_IfActionHasObsoleteAttribute()
        {
            var subject = Subject(
                apiDescriptions: new[]
                {
                    ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionWithObsoleteAttribute), groupName: "v1", httpMethod: "POST", relativePath: "resource"),
                }
            );

            var document = subject.GetSwagger("v1");

            Assert.True(document.Paths["/resource"].Operations[OperationType.Post].Deprecated);
        }

        [Theory]
        [InlineData(nameof(BindingSource.Query), ParameterLocation.Query)]
        [InlineData(nameof(BindingSource.Header), ParameterLocation.Header)]
        [InlineData(nameof(BindingSource.Path), ParameterLocation.Path)]
        [InlineData(null, ParameterLocation.Query)]
        public void GetSwagger_GeneratesParameters_ForApiParametersThatAreNotBoundToBodyOrForm(
            string bindingSourceId,
            ParameterLocation expectedParameterLocation)
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

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            var parameter = Assert.Single(operation.Parameters);
            Assert.Equal(expectedParameterLocation, parameter.In);
        }

        [Fact]
        public void GetSwagger_IgnoresOperations_IfOperationHasSwaggerIgnoreAttribute()
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

            Assert.Empty(document.Paths);
        }

        [Fact]
        public void GetSwagger_IgnoresParameters_IfActionParameterHasBindNeverAttribute()
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

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.Empty(operation.Parameters);
        }

        [Fact]
        public void GetSwagger_IgnoresParameters_IfActionParameterHasSwaggerIgnoreAttribute()
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

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.Empty(operation.Parameters);
        }

        [Theory]
        [InlineData(nameof(FakeController.ActionWithAcceptFromHeaderParameter))]
        [InlineData(nameof(FakeController.ActionWithContentTypeFromHeaderParameter))]
        [InlineData(nameof(FakeController.ActionWithAuthorizationFromHeaderParameter))]
        public void GetSwagger_IgnoresParameters_IfActionParameterIsIllegalHeaderParameter(string action)
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

            var operation = document.Paths["/resource"].Operations[OperationType.Get];
            var parameter = Assert.Single(operation.Parameters);
            Assert.Equal("param", parameter.Name);
        }

        [Theory]
        [InlineData(nameof(FakeController.ActionWithAcceptFromHeaderParameter))]
        [InlineData(nameof(FakeController.ActionWithContentTypeFromHeaderParameter))]
        [InlineData(nameof(FakeController.ActionWithAuthorizationFromHeaderParameter))]
        public void GetSwagger_GenerateParametersSchemas_IfActionParameterIsIllegalHeaderParameterWithProvidedOpenApiOperation(string action)
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

            var operation = document.Paths["/resource"].Operations[OperationType.Get];
            Assert.Null(operation.Parameters.Single(p => p.Name == illegalParameterName).Schema);
            Assert.NotNull(operation.Parameters.Single(p => p.Name == "param").Schema);
        }

        [Fact]
        public void GetSwagger_SetsParameterRequired_IfApiParameterIsBoundToPath()
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

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.True(operation.Parameters.First().Required);
        }

        [Theory]
        [InlineData(nameof(FakeController.ActionWithParameter), false)]
        [InlineData(nameof(FakeController.ActionWithParameterWithRequiredAttribute), true)]
        [InlineData(nameof(FakeController.ActionWithParameterWithBindRequiredAttribute), true)]
        public void GetSwagger_SetsParameterRequired_IfActionParameterHasRequiredOrBindRequiredAttribute(
            string actionName,
            bool expectedRequired)
        {
            var subject = Subject(
                apiDescriptions: new[]
                {
                    ApiDescriptionFactory.Create(
                        methodInfo: typeof(FakeController).GetMethod(actionName),
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

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            var parameter = Assert.Single(operation.Parameters);
            Assert.Equal(expectedRequired, parameter.Required);
        }

#if NET7_0_OR_GREATER
        [Fact]
        public void GetSwagger_SetsParameterRequired_IfActionParameterHasRequiredMember()
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

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            var parameter = Assert.Single(operation.Parameters);
            Assert.True(parameter.Required);
        }
#endif

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void GetSwagger_SetsParameterRequired_ForNonControllerActionDescriptor_IfApiParameterDescriptionForBodyIsRequired(bool isRequired)
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
                IsRequired = isRequired,
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

            Assert.Equal(isRequired, document.Paths["/resource"].Operations[OperationType.Post].RequestBody.Required);
        }

        [Fact]
        public void GetSwagger_SetsParameterTypeToString_IfApiParameterHasNoCorrespondingActionParameter()
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

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            var parameter = Assert.Single(operation.Parameters);
            Assert.Equal("string", parameter.Schema.Type);
        }

        [Fact]
        public void GetSwagger_GeneratesRequestBody_ForFirstApiParameterThatIsBoundToBody()
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

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.NotNull(operation.RequestBody);
            Assert.Equal(new[] { "application/json" }, operation.RequestBody.Content.Keys);
            var mediaType = operation.RequestBody.Content["application/json"];
            Assert.NotNull(mediaType.Schema);
        }

        [Theory]
        [InlineData(nameof(FakeController.ActionWithParameter), false)]
        [InlineData(nameof(FakeController.ActionWithParameterWithRequiredAttribute), true)]
        [InlineData(nameof(FakeController.ActionWithParameterWithBindRequiredAttribute), true)]
        public void GetSwagger_SetsRequestBodyRequired_IfActionParameterHasRequiredOrBindRequiredMetadata(
            string actionName,
            bool expectedRequired)
        {
            var subject = Subject(
                apiDescriptions: new[]
                {
                    ApiDescriptionFactory.Create(
                        methodInfo: typeof(FakeController).GetMethod(actionName),
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

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.Equal(expectedRequired, operation.RequestBody.Required);
        }

        [Fact]
        public void GetSwagger_GeneratesRequestBody_ForApiParametersThatAreBoundToForm()
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

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.NotNull(operation.RequestBody);
            Assert.Equal(new[] { "multipart/form-data" }, operation.RequestBody.Content.Keys);
            var mediaType = operation.RequestBody.Content["multipart/form-data"];
            Assert.NotNull(mediaType.Schema);
            Assert.Equal(new[] { "param1", "param2" }, mediaType.Schema.Properties.Keys);
            Assert.NotNull(mediaType.Encoding);
        }

        [Theory]
        [InlineData("Body")]
        [InlineData("Form")]
        public void GetSwagger_SetsRequestBodyContentTypesFromAttribute_IfActionHasConsumesAttribute(
            string bindingSourceId)
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

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.Equal(new[] { "application/someMediaType" }, operation.RequestBody.Content.Keys);
        }

        [Fact]
        public void GetSwagger_GeneratesResponses_ForSupportedResponseTypes()
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

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.Equal(new[] { "200", "400", "422", "default" }, operation.Responses.Keys);
            var response200 = operation.Responses["200"];
            Assert.Equal("OK", response200.Description);
            Assert.Equal(new[] { "application/json" }, response200.Content.Keys);
            var response400 = operation.Responses["400"];
            Assert.Equal("Bad Request", response400.Description);
            Assert.Empty(response400.Content.Keys);
            var response422 = operation.Responses["422"];
            Assert.Equal("Unprocessable Content", response422.Description);
            Assert.Empty(response422.Content.Keys);
            var responseDefault = operation.Responses["default"];
            Assert.Equal("Error", responseDefault.Description);
            Assert.Empty(responseDefault.Content.Keys);
        }

        [Fact]
        public void GetSwagger_SetsResponseContentType_WhenActionHasFileResult()
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

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            var content = operation.Responses["200"].Content.FirstOrDefault();
            Assert.Equal("application/zip", content.Key);
            Assert.Equal("binary", content.Value.Schema.Format);
            Assert.Equal("string", content.Value.Schema.Type);
        }

        [Fact]
        public void GetSwagger_SetsResponseContentTypesFromAttribute_IfActionHasProducesAttribute()
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

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.Equal(new[] { "application/someMediaType" }, operation.Responses["200"].Content.Keys);
        }

        [Fact]
        public void GetSwagger_ThrowsUnknownSwaggerDocumentException_IfProvidedDocumentNameNotRegistered()
        {
            var subject = Subject(
                apiDescriptions: new[]
                {
                    ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource"),
                }
            );

            var exception = Assert.Throws<UnknownSwaggerDocument>(() => subject.GetSwagger("v2"));
            Assert.Equal(
                "Unknown Swagger document - \"v2\". Known Swagger documents: \"v1\"",
                exception.Message);
        }

        [Fact]
        public void GetSwagger_ThrowsSwaggerGeneratorException_IfActionHasNoHttpBinding()
        {
            var subject = Subject(
                apiDescriptions: new[]
                {
                    ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: null, relativePath: "resource")
                }
            );

            var exception = Assert.Throws<SwaggerGeneratorException>(() => subject.GetSwagger("v1"));
            Assert.Equal(
                "Ambiguous HTTP method for action - Swashbuckle.AspNetCore.SwaggerGen.Test.FakeController.ActionWithNoParameters (Swashbuckle.AspNetCore.SwaggerGen.Test). " +
                "Actions require an explicit HttpMethod binding for Swagger/OpenAPI 3.0",
                exception.Message);
        }

        [Fact]
        public void GetSwagger_ThrowsSwaggerGeneratorException_IfActionsHaveConflictingHttpMethodAndPath()
        {
            var subject = Subject(
                apiDescriptions: new[]
                {
                    ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource"),

                    ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource")
                }
            );

            var exception = Assert.Throws<SwaggerGeneratorException>(() => subject.GetSwagger("v1"));
            Assert.Equal(
                "Conflicting method/path combination \"POST resource\" for actions - " +
                "Swashbuckle.AspNetCore.SwaggerGen.Test.FakeController.ActionWithNoParameters (Swashbuckle.AspNetCore.SwaggerGen.Test)," +
                "Swashbuckle.AspNetCore.SwaggerGen.Test.FakeController.ActionWithNoParameters (Swashbuckle.AspNetCore.SwaggerGen.Test). " +
                "Actions require a unique method/path combination for Swagger/OpenAPI 3.0. Use ConflictingActionsResolver as a workaround",
                exception.Message);
        }

        [Fact]
        public void GetSwagger_SupportsOption_IgnoreObsoleteActions()
        {
            var subject = Subject(
                apiDescriptions: new[]
                {
                    ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource"),

                    ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionWithObsoleteAttribute), groupName: "v1", httpMethod: "GET", relativePath: "resource")
                },
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    IgnoreObsoleteActions = true
                }
            );

            var document = subject.GetSwagger("v1");

            Assert.Equal(new[] { "/resource" }, document.Paths.Keys.ToArray());
            Assert.Equal(new[] { OperationType.Post }, document.Paths["/resource"].Operations.Keys);
        }

        [Fact]
        public void GetSwagger_SupportsOption_SortKeySelector()
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

            Assert.Equal(new[] { "/resource1", "/resource2", "/resource3" }, document.Paths.Keys.ToArray());
        }

        [Fact]
        public void GetSwagger_SupportsOption_TagSelector()
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

            Assert.Equal(new[] { "resource" }, document.Paths["/resource"].Operations[OperationType.Post].Tags.Select(t => t.Name));
        }

        [Fact]
        public void GetSwagger_CanReadTagsFromMetadata()
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

            Assert.Equal(new[] { "Some", "Tags", "Here" }, document.Paths["/resource"].Operations[OperationType.Post].Tags.Select(t => t.Name));
        }

#if NET7_0_OR_GREATER
        [Fact]
        public void GetSwagger_CanReadEndpointSummaryFromMetadata()
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

            Assert.Equal("A Test Summary", document.Paths["/resource"].Operations[OperationType.Post].Summary);
        }

        [Fact]
        public void GetSwagger_CanReadEndpointDescriptionFromMetadata()
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

            Assert.Equal("A Test Description", document.Paths["/resource"].Operations[OperationType.Post].Description);
        }
#endif

        [Fact]
        public void GetSwagger_SupportsOption_ConflictingActionsResolver()
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

            Assert.Equal(new[] { "/resource" }, document.Paths.Keys.ToArray());
            Assert.Equal(new[] { OperationType.Post }, document.Paths["/resource"].Operations.Keys);
        }

        [Theory]
        [InlineData("SomeParam", "someParam")]
        [InlineData("FooBar.SomeParam", "fooBar.someParam")]
        [InlineData("A.B", "a.b")]
        [InlineData("", "")]
        [InlineData(null, null)]
        public void GetSwagger_SupportsOption_DescribeAllParametersInCamelCase(
            string parameterName,
            string expectedOpenApiParameterName)
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
                                Name = parameterName,
                                Source = BindingSource.Path
                            }
                        })
                },
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    DescribeAllParametersInCamelCase = true
                }
            );

            var document = subject.GetSwagger("v1");

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            var parameter = Assert.Single(operation.Parameters);
            Assert.Equal(expectedOpenApiParameterName, parameter.Name);
        }

        [Fact]
        public void GetSwagger_SupportsOption_Servers()
        {
            var subject = Subject(
                apiDescriptions: new ApiDescription[] { },
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer { Url = "http://tempuri.org/api" }
                    }
                }
            );

            var document = subject.GetSwagger("v1");

            var server = Assert.Single(document.Servers);
            Assert.Equal("http://tempuri.org/api", server.Url);
        }

        [Fact]
        public void GetSwagger_SupportsOption_SecuritySchemes()
        {
            var subject = Subject(
                apiDescriptions: new ApiDescription[] { },
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
                    {
                        ["basic"] = new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "basic" }
                    }
                }
            );

            var document = subject.GetSwagger("v1");

            Assert.Equal(new[] { "basic" }, document.Components.SecuritySchemes.Keys);
        }

        [Theory]
        [InlineData(false, new string[] { })]
        [InlineData(true, new string[] { "Bearer" })]
        public async Task GetSwagger_SupportsOption_InferSecuritySchemes(
            bool inferSecuritySchemes,
            string[] expectedSecuritySchemeNames)

        {
            var subject = Subject(
                apiDescriptions: new ApiDescription[] { },
                authenticationSchemes: new[] {
                    new AuthenticationScheme("Bearer", null, typeof(IAuthenticationHandler)),
                    new AuthenticationScheme("Cookies", null, typeof(IAuthenticationHandler))
                },
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    InferSecuritySchemes = inferSecuritySchemes
                }
            );

            var document = await subject.GetSwaggerAsync("v1");

            Assert.Equal(expectedSecuritySchemeNames, document.Components.SecuritySchemes.Keys);
        }

        [Theory]
        [InlineData(false, new string[] { })]
        [InlineData(true, new string[] { "Bearer", "Cookies" })]
        public async Task GetSwagger_SupportsOption_SecuritySchemesSelector(
            bool inferSecuritySchemes,
            string[] expectedSecuritySchemeNames)

        {
            var subject = Subject(
                apiDescriptions: new ApiDescription[] { },
                authenticationSchemes: new[] {
                    new AuthenticationScheme("Bearer", null, typeof(IAuthenticationHandler)),
                    new AuthenticationScheme("Cookies", null, typeof(IAuthenticationHandler))
                },
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    InferSecuritySchemes = inferSecuritySchemes,
                    SecuritySchemesSelector = (authenticationSchemes) =>
                        authenticationSchemes
                            .ToDictionary(
                                (authScheme) => authScheme.Name,
                                (authScheme) => new OpenApiSecurityScheme())
                }
            );

            var document = await subject.GetSwaggerAsync("v1");

            Assert.Equal(expectedSecuritySchemeNames, document.Components.SecuritySchemes.Keys);
        }

        [Fact]
        public void GetSwagger_SupportsOption_ParameterFilters()
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
                            new ApiParameterDescription { Name = "param", Source = BindingSource.Query }
                        })
                },
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    ParameterFilters = new List<IParameterFilter>
                    {
                        new TestParameterFilter()
                    }
                }
            );

            var document = subject.GetSwagger("v1");

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.Equal(2, operation.Parameters[0].Extensions.Count);
            Assert.Equal("bar", ((OpenApiString)operation.Parameters[0].Extensions["X-foo"]).Value);
            Assert.Equal("v1", ((OpenApiString)operation.Parameters[0].Extensions["X-docName"]).Value);
        }

        [Fact]
        public void GetSwagger_SupportsOption_RequestBodyFilters()
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
                            new ApiParameterDescription { Name = "param", Source = BindingSource.Body }
                        })
                },
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    RequestBodyFilters = new List<IRequestBodyFilter>
                    {
                        new TestRequestBodyFilter()
                    }
                }
            );

            var document = subject.GetSwagger("v1");

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.Equal(2, operation.RequestBody.Extensions.Count);
            Assert.Equal("bar", ((OpenApiString)operation.RequestBody.Extensions["X-foo"]).Value);
            Assert.Equal("v1", ((OpenApiString)operation.RequestBody.Extensions["X-docName"]).Value);
        }

        [Fact]
        public void GetSwagger_SupportsOption_OperationFilters()
        {
            var subject = Subject(
                apiDescriptions: new[]
                {
                    ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource")
                },
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    OperationFilters = new List<IOperationFilter>
                    {
                        new TestOperationFilter()
                    }
                }
            );

            var document = subject.GetSwagger("v1");

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.Equal(2, operation.Extensions.Count);
            Assert.Equal("bar", ((OpenApiString)operation.Extensions["X-foo"]).Value);
            Assert.Equal("v1", ((OpenApiString)operation.Extensions["X-docName"]).Value);
        }

        [Fact]
        public void GetSwagger_SupportsOption_DocumentFilters()
        {
            var subject = Subject(
                apiDescriptions: new ApiDescription[] { },
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    DocumentFilters = new List<IDocumentFilter>
                    {
                        new TestDocumentFilter()
                    }
                }
            );

            var document = subject.GetSwagger("v1");

            Assert.Equal(2, document.Extensions.Count);
            Assert.Equal("bar", ((OpenApiString)document.Extensions["X-foo"]).Value);
            Assert.Equal("v1", ((OpenApiString)document.Extensions["X-docName"]).Value);
            Assert.Contains("ComplexType", document.Components.Schemas.Keys);
        }

        [Fact]
        public async Task GetSwaggerAsync_SupportsOption_OperationFilters()
        {
            var subject = Subject(
                apiDescriptions: new[]
                {
                    ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource")
                },
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    OperationFilters = new List<IOperationFilter>
                    {
                        new TestOperationFilter()
                    }
                }
            );

            var document = await subject.GetSwaggerAsync("v1");

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.Equal(2, operation.Extensions.Count);
            Assert.Equal("bar", ((OpenApiString)operation.Extensions["X-foo"]).Value);
            Assert.Equal("v1", ((OpenApiString)operation.Extensions["X-docName"]).Value);
        }

        [Fact]
        public async Task GetSwaggerAsync_SupportsOption_OperationAsyncFilters()
        {
            var subject = Subject(
                apiDescriptions:
                [
                    ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource")
                ],
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    OperationAsyncFilters =
                    [
                        new TestOperationFilter()
                    ]
                }
            );

            var document = await subject.GetSwaggerAsync("v1");

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.Equal(2, operation.Extensions.Count);
            Assert.Equal("bar", ((OpenApiString)operation.Extensions["X-foo"]).Value);
            Assert.Equal("v1", ((OpenApiString)operation.Extensions["X-docName"]).Value);
        }

        [Fact]
        public async Task GetSwaggerAsync_SupportsOption_DocumentAsyncFilters()
        {
            var subject = Subject(
                apiDescriptions: [],
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    DocumentAsyncFilters =
                    [
                        new TestDocumentFilter()
                    ]
                }
            );

            var document = await subject.GetSwaggerAsync("v1");

            Assert.Equal(2, document.Extensions.Count);
            Assert.Equal("bar", ((OpenApiString)document.Extensions["X-foo"]).Value);
            Assert.Equal("v1", ((OpenApiString)document.Extensions["X-docName"]).Value);
            Assert.Contains("ComplexType", document.Components.Schemas.Keys);
        }

        [Fact]
        public async Task GetSwaggerAsync_SupportsOption_DocumentFilters()
        {
            var subject = Subject(
                apiDescriptions: Array.Empty<ApiDescription>(),
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    DocumentFilters =
                    [
                        new TestDocumentFilter()
                    ]
                }
            );

            var document = await subject.GetSwaggerAsync("v1");

            Assert.Equal(2, document.Extensions.Count);
            Assert.Equal("bar", ((OpenApiString)document.Extensions["X-foo"]).Value);
            Assert.Equal("v1", ((OpenApiString)document.Extensions["X-docName"]).Value);
            Assert.Contains("ComplexType", document.Components.Schemas.Keys);
        }

        [Fact]
        public async Task GetSwaggerAsync_SupportsOption_RequestBodyAsyncFilters()
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
                            new ApiParameterDescription { Name = "param", Source = BindingSource.Body }
                        })
                },
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    RequestBodyAsyncFilters =
                    [
                        new TestRequestBodyFilter()
                    ]
                }
            );

            var document = await subject.GetSwaggerAsync("v1");

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.Equal(2, operation.RequestBody.Extensions.Count);
            Assert.Equal("bar", ((OpenApiString)operation.RequestBody.Extensions["X-foo"]).Value);
            Assert.Equal("v1", ((OpenApiString)operation.RequestBody.Extensions["X-docName"]).Value);
        }

        [Fact]
        public async Task GetSwaggerAsync_SupportsOption_RequestBodyFilters()
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
                            new ApiParameterDescription { Name = "param", Source = BindingSource.Body }
                        })
                },
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    RequestBodyFilters = new List<IRequestBodyFilter>
                    {
                        new TestRequestBodyFilter()
                    }
                }
            );

            var document = await subject.GetSwaggerAsync("v1");

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.Equal(2, operation.RequestBody.Extensions.Count);
            Assert.Equal("bar", ((OpenApiString)operation.RequestBody.Extensions["X-foo"]).Value);
            Assert.Equal("v1", ((OpenApiString)operation.RequestBody.Extensions["X-docName"]).Value);
        }

        [Fact]
        public async Task GetSwaggerAsync_SupportsOption_ParameterFilters()
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
                            new ApiParameterDescription { Name = "param", Source = BindingSource.Query }
                        })
                },
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    ParameterFilters = new List<IParameterFilter>
                    {
                        new TestParameterFilter()
                    }
                }
            );

            var document = await subject.GetSwaggerAsync("v1");

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.Equal(2, operation.Parameters[0].Extensions.Count);
            Assert.Equal("bar", ((OpenApiString)operation.Parameters[0].Extensions["X-foo"]).Value);
            Assert.Equal("v1", ((OpenApiString)operation.Parameters[0].Extensions["X-docName"]).Value);
        }

        [Fact]
        public async Task GetSwaggerAsync_SupportsOption_ParameterAsyncFilters()
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
                            new ApiParameterDescription { Name = "param", Source = BindingSource.Query }
                        })
                },
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    },
                    ParameterAsyncFilters =
                    [
                        new TestParameterFilter()
                    ]
                }
            );

            var document = await subject.GetSwaggerAsync("v1");

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.Equal(2, operation.Parameters[0].Extensions.Count);
            Assert.Equal("bar", ((OpenApiString)operation.Parameters[0].Extensions["X-foo"]).Value);
            Assert.Equal("v1", ((OpenApiString)operation.Parameters[0].Extensions["X-docName"]).Value);
        }

        [Theory]
        [InlineData("connect")]
        [InlineData("CONNECT")]
        [InlineData("FOO")]
        public void GetSwagger_GeneratesSwaggerDocument_ThrowsIfHttpMethodNotSupported(string httpMethod)
        {
            var subject = Subject(
                apiDescriptions: new[]
                {
                    ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: httpMethod, relativePath: "resource"),
                },
                options: new SwaggerGeneratorOptions
                {
                    SwaggerDocs = new Dictionary<string, OpenApiInfo>
                    {
                        ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    }
                }
            );

            var exception = Assert.Throws<SwaggerGeneratorException>(() => subject.GetSwagger("v1"));
            Assert.Equal($"The \"{httpMethod}\" HTTP method is not supported.", exception.Message);
        }

        [Fact]
        public void GetSwagger_Throws_Exception_When_FromForm_Attribute_Used_With_IFormFile()
        {
            var parameterInfo = typeof(FakeController)
                .GetMethod(nameof(FakeController.ActionHavingIFormFileParamWithFromFormAttribute))
                .GetParameters()[0];

            var subject = Subject(
                apiDescriptions: new[]
                {
                   ApiDescriptionFactory.Create<FakeController>(
                        c => nameof(c.ActionHavingIFormFileParamWithFromFormAttribute),
                        groupName: "v1",
                        httpMethod: "POST",
                        relativePath: "resource",
                        parameterDescriptions: new[]
                        {
                            new ApiParameterDescription
                            {
                                Name = "fileUpload", // Name of the parameter
                                Type = typeof(IFormFile), // Type of the parameter
                                ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo }
                            }
                        })
                }
            );

            Assert.Throws<SwaggerGeneratorException>(() => subject.GetSwagger("v1"));
        }

        [Fact]
        public void GetSwagger_Works_As_Expected_When_FromForm_Attribute_Not_Used_With_IFormFile()
        {
            var paraminfo = typeof(FakeController)
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
                                ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = paraminfo }
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
            Assert.Equal("V1", document.Info.Version);
            Assert.Equal("Test API", document.Info.Title);
            Assert.Equal(new[] { "/resource" }, document.Paths.Keys.ToArray());

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.NotNull(operation.Parameters);
            Assert.Equal(2, operation.Parameters.Count);
            Assert.Equal("param1", operation.Parameters[0].Name);
            Assert.Equal("param2", operation.Parameters[1].Name);
        }

        [Fact]
        public void GetSwagger_Works_As_Expected_When_FromForm_Attribute_With_SwaggerIgnore()
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

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.NotNull(operation.RequestBody);
            Assert.Equal(new[] { "multipart/form-data" }, operation.RequestBody.Content.Keys);
            var mediaType = operation.RequestBody.Content["multipart/form-data"];
            Assert.NotNull(mediaType.Schema);
            Assert.Equal(new[] { nameof(SwaggerIngoreAnnotatedType.NotIgnoredString) }, mediaType.Schema.Properties.Keys);
            Assert.NotNull(mediaType.Encoding);
            Assert.Equal(new[] { nameof(SwaggerIngoreAnnotatedType.NotIgnoredString) }, mediaType.Encoding.Keys);
        }

        [Fact]
        public void GetSwagger_Copies_Description_From_GeneratedSchema()
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
                        parameterDescriptions: new[]
                        {
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
                        })
               ],
               schemaFilters: [new TestEnumSchemaFilter()]
           );
            var document = subject.GetSwagger("v1");

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.NotEmpty(operation.Parameters);
            Assert.Equal(nameof(TypeWithDefaultAttributeOnEnum.EnumWithDefault), operation.Parameters[0].Name);
            Assert.Equal(document.Components.Schemas[nameof(IntEnum)].Description, operation.Parameters[0].Description);
            Assert.Equal(nameof(TypeWithDefaultAttributeOnEnum.EnumArrayWithDefault), operation.Parameters[1].Name);
            Assert.Equal(document.Components.Schemas[nameof(IntEnum)].Description, operation.Parameters[1].Description);
        }

        [Fact]
        public void GetSwagger_GenerateConsumesSchemas_ForProvidedOpenApiOperationWithSeveralFromForms()
        {
            var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithConsumesAttribute));
            var actionDescriptor = new ActionDescriptor
            {
                EndpointMetadata =
                [
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

            Assert.Equal("OperationIdSetInMetadata", document.Paths["/resource"].Operations[OperationType.Post].OperationId);
            var content = Assert.Single(document.Paths["/resource"].Operations[OperationType.Post].RequestBody.Content);
            Assert.Equal("application/someMediaType", content.Key);
            Assert.NotNull(content.Value.Schema);
            Assert.NotNull(content.Value.Schema.AllOf);
            Assert.Equal("TestDto", content.Value.Schema.AllOf[0].Reference.Id);
            Assert.Equal("TypeWithDefaultAttributeOnEnum", content.Value.Schema.AllOf[1].Reference.Id);
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
}
