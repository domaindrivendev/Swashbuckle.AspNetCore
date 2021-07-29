using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Any;
using Xunit;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.TestSupport;

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
                        [ "v1" ] = new OpenApiInfo { Version = "V1", Title = "Test API" }
                    }
                }
            );

            var document = subject.GetSwagger("v1");

            Assert.Equal("V1", document.Info.Version);
            Assert.Equal("Test API", document.Info.Title);
            Assert.Equal(new[] { "/resource" }, document.Paths.Keys.ToArray());
            Assert.Equal(new[] { OperationType.Post, OperationType.Get }, document.Paths["/resource"].Operations.Keys);
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
            Assert.Equal(1, operation.Parameters.Count);
            Assert.Equal(expectedParameterLocation, operation.Parameters.First().In);
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
            Assert.Equal(0, operation.Parameters.Count);
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
            Assert.Equal(1, operation.Parameters.Count);
            Assert.Equal(expectedRequired, operation.Parameters.First().Required);
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
            Assert.Equal(1, operation.Parameters.Count);
            Assert.Equal("string", operation.Parameters.First().Schema.Type);
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
                                IsDefaultResponse = true
                            }

                        }
                    )
                }
            );

            var document = subject.GetSwagger("v1");

            var operation = document.Paths["/resource"].Operations[OperationType.Post];
            Assert.Equal(new[] { "200", "400", "default" }, operation.Responses.Keys);
            var response200 = operation.Responses["200"];
            Assert.Equal("Success", response200.Description);
            Assert.Equal(new[] { "application/json" }, response200.Content.Keys);
            var response400 = operation.Responses["400"];
            Assert.Equal("Bad Request", response400.Description);
            Assert.Empty(response400.Content.Keys);
            var responseDefault = operation.Responses["default"];
            Assert.Equal("Error", responseDefault.Description);
            Assert.Empty(responseDefault.Content.Keys);
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
            Assert.Equal(1, operation.Parameters.Count);
            Assert.Equal(expectedOpenApiParameterName, operation.Parameters.First().Name);
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

            Assert.Equal(1, document.Servers.Count);
            Assert.Equal("http://tempuri.org/api", document.Servers.First().Url);
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
            Assert.Equal(2, operation.Parameters[0].Extensions.Count());
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

        private SwaggerGenerator Subject(IEnumerable<ApiDescription> apiDescriptions, SwaggerGeneratorOptions options = null)
        {
            return new SwaggerGenerator(
                options ?? DefaultOptions,
                new FakeApiDescriptionGroupCollectionProvider(apiDescriptions),
                new SchemaGenerator(new SchemaGeneratorOptions(), new JsonSerializerDataContractResolver(new JsonSerializerOptions()))
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