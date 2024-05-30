﻿using System.Collections.Generic;
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
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.TestSupport;
using VerifyXunit;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class VarifyTests
    {
        [Fact]
        public Task GetSwagger_GeneratesSwaggerDocument_ForApiDescriptionsWithMatchingGroupName()
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
        public Task GetSwagger_GeneratesSwaggerDocument_ForActionWithRouteNameMetadata()
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
        public Task GetSwagger_GeneratesSwaggerDocument_ForActionWithEndpointNameMetadata()
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
        public Task GetSwagger_GeneratesSwaggerDocument_ForActionWithProvidedOpenApiMetadata()
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

        [Theory]
        [InlineData(nameof(FakeController.ActionWithAcceptFromHeaderParameter))]
        [InlineData(nameof(FakeController.ActionWithContentTypeFromHeaderParameter))]
        [InlineData(nameof(FakeController.ActionWithAuthorizationFromHeaderParameter))]
        public void GetSwagger_GeneratesSwaggerDocument_ForActionsWithIllegalHeaderParameters(string action)
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
}
