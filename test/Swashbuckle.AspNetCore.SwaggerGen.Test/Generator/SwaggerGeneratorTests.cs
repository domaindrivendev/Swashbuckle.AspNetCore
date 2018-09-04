using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class SwaggerGeneratorTests
    {
        [Fact]
        public void GetSwagger_RequiresTargetDocumentToBeSpecifiedBySettings()
        {
            var subject = Subject(setupAction: (c) => c.SwaggerDocs.Clear());

            Assert.Throws<UnknownSwaggerDocument>(() => subject.GetSwagger("v1"));
        }

        [Fact]
        public void GetSwagger_GeneratesOneOrMoreDocuments_AsSpecifiedBySettings()
        {
            var v1Info = new Info { Version = "v2", Title = "API V2" };
            var v2Info = new Info { Version = "v1", Title = "API V1" };

            var subject = Subject(
                setupApis: apis =>
                {
                    apis.Add("GET", "v1/collection", nameof(FakeController.ReturnsEnumerable));
                    apis.Add("GET", "v2/collection", nameof(FakeController.ReturnsEnumerable));
                },
                setupAction: c =>
                {
                    c.SwaggerDocs.Clear();
                    c.SwaggerDocs.Add("v1", v1Info);
                    c.SwaggerDocs.Add("v2", v2Info);
                    c.DocInclusionPredicate = (docName, api) => api.RelativePath.StartsWith(docName);
                });

            var v1Swagger = subject.GetSwagger("v1");
            var v2Swagger = subject.GetSwagger("v2");

            Assert.Equal(new[] { "/v1/collection" }, v1Swagger.Paths.Keys.ToArray());
            Assert.Equal(v1Info, v1Swagger.Info);
            Assert.Equal(new[] { "/v2/collection" }, v2Swagger.Paths.Keys.ToArray());
            Assert.Equal(v2Info, v2Swagger.Info);
        }

        [Fact]
        public void GetSwagger_GeneratesPathItem_PerRelativePathSansQueryString()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection1", nameof(FakeController.ReturnsEnumerable))
                .Add("GET", "collection1/{id}", nameof(FakeController.ReturnsComplexType))
                .Add("GET", "collection2", nameof(FakeController.AcceptsStringFromQuery))
                .Add("PUT", "collection2", nameof(FakeController.ReturnsVoid))
                .Add("GET", "collection2/{id}", nameof(FakeController.ReturnsComplexType))
            );

            var swagger = subject.GetSwagger("v1");

            Assert.Equal(new[]
                {
                    "/collection1",
                    "/collection1/{id}",
                    "/collection2",
                    "/collection2/{id}"
                },
                swagger.Paths.Keys.ToArray());
        }

        [Fact]
        public void GetSwagger_GeneratesOperation_PerHttpMethodPerRelativePathSansQueryString()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(FakeController.ReturnsEnumerable))
                .Add("PUT", "collection/{id}", nameof(FakeController.AcceptsComplexTypeFromBody))
                .Add("POST", "collection", nameof(FakeController.AcceptsComplexTypeFromBody))
                .Add("DELETE", "collection/{id}", nameof(FakeController.ReturnsVoid))
                .Add("PATCH", "collection/{id}", nameof(FakeController.AcceptsComplexTypeFromBody))
            // TODO: OPTIONS & HEAD
            );

            var swagger = subject.GetSwagger("v1");

            // GET collection
            var operation = swagger.Paths["/collection"].Get;
            Assert.NotNull(operation);
            Assert.Empty(operation.Consumes);
            Assert.Equal(new[] { "application/json", "text/json" }, operation.Produces.ToArray());
            Assert.Null(operation.Deprecated);
            // PUT collection/{id}
            operation = swagger.Paths["/collection/{id}"].Put;
            Assert.NotNull(operation);
            Assert.Equal(new[] { "application/json", "text/json", "application/*+json" }, operation.Consumes.ToArray());
            Assert.Empty(operation.Produces.ToArray());
            Assert.Null(operation.Deprecated);
            // POST collection
            operation = swagger.Paths["/collection"].Post;
            Assert.NotNull(operation);
            Assert.Equal(new[] { "application/json", "text/json", "application/*+json" }, operation.Consumes.ToArray());
            Assert.Empty(operation.Produces.ToArray());
            Assert.Null(operation.Deprecated);
            // DELETE collection/{id}
            operation = swagger.Paths["/collection/{id}"].Delete;
            Assert.NotNull(operation);
            Assert.Empty(operation.Consumes.ToArray());
            Assert.Empty(operation.Produces.ToArray());
            Assert.Null(operation.Deprecated);
            // PATCH collection
            operation = swagger.Paths["/collection/{id}"].Patch;
            Assert.NotNull(operation);
            Assert.Equal(new[] { "application/json", "text/json", "application/*+json" }, operation.Consumes.ToArray());
            Assert.Empty(operation.Produces.ToArray());
            Assert.Null(operation.Deprecated);
        }

        [Theory]
        [InlineData("api/products", "ApiProductsGet")]
        [InlineData("addresses/validate", "AddressesValidateGet")]
        [InlineData("carts/{cartId}/items/{id}", "CartsByCartIdItemsByIdGet")]
        public void GetSwagger_GeneratesOperationIds_AccordingToRouteTemplateAndHttpMethod(
            string routeTemplate,
            string expectedOperationId
        )
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", routeTemplate, nameof(FakeController.AcceptsNothing)));

            var swagger = subject.GetSwagger("v1");

            Assert.Equal(expectedOperationId, swagger.Paths["/" + routeTemplate].Get.OperationId);
        }

        [Fact]
        public void GetSwagger_SetsConsumesFromConsumesAttribute_IfPresent()
        {
            var subject = Subject(setupApis: apis =>
                apis.Add("POST", "resource", nameof(FakeController.AnnotatedWithConsumes)));

            var swagger = subject.GetSwagger("v1");

            var operation = swagger.Paths["/resource"].Post;
            Assert.Equal(new[] { "application/xml" }, operation.Consumes.ToArray());
        }

        [Fact]
        public void GetSwagger_SetsConsumesToMultipartFormData_IfOperationContainsFormParams()
        {
            var subject = Subject(setupApis: apis =>
                apis.Add("POST", "form", nameof(FakeController.AcceptsStringFromForm)));

            var swagger = subject.GetSwagger("v1");

            var operation = swagger.Paths["/form"].Post;
            Assert.Equal(new[] { "multipart/form-data" }, operation.Consumes.ToArray());
        }

        [Fact]
        public void GetSwagger_SetsProducesFromProducesAttribute_IfPresent()
        {
            var subject = Subject(setupApis: apis =>
                apis.Add("GET", "resource", nameof(FakeController.AnnotatedWithProduces)));

            var swagger = subject.GetSwagger("v1");

            var operation = swagger.Paths["/resource"].Get;
            Assert.Equal(new[] { "application/xml" }, operation.Produces.ToArray());
        }

        [Fact]
        public void GetSwagger_SetsCollectionFormatMulti_ForQueryOrHeaderBoundArrayParams()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "resource", nameof(FakeController.AcceptsArrayFromQuery)));

            var swagger = subject.GetSwagger("v1");

            var param = (NonBodyParameter)swagger.Paths["/resource"].Get.Parameters.First();
            Assert.Equal("multi", param.CollectionFormat);
        }

        [Fact]
        public void GetSwagger_GeneratesBodyParams_ForParamsBoundToBody()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("POST", "collection", nameof(FakeController.AcceptsComplexTypeFromBody)));

            var swagger = subject.GetSwagger("v1");

            var param = swagger.Paths["/collection"].Post.Parameters.First();
            Assert.IsAssignableFrom<BodyParameter>(param);
            var bodyParam = param as BodyParameter;
            Assert.Equal("param", bodyParam.Name);
            Assert.Equal("body", bodyParam.In);
            Assert.NotNull(bodyParam.Schema);
            Assert.Equal("#/definitions/ComplexType", bodyParam.Schema.Ref);
            Assert.Contains("ComplexType", swagger.Definitions.Keys);
        }

        [Theory]
        [InlineData("resource/{param}", nameof(FakeController.AcceptsStringFromRoute), "path", "string")]
        [InlineData("resource", nameof(FakeController.AcceptsIntegerFromQuery), "query", "integer")]
        [InlineData("resource", nameof(FakeController.AcceptsStringFromHeader), "header", "string")]
        [InlineData("resource", nameof(FakeController.AcceptsStringFromForm), "formData", "string")]
        [InlineData("resource", nameof(FakeController.AcceptsIFormFile), "formData", "file")]
        public void GetSwagger_GeneratesNonBodyParams_ForParamsBoundToSourcesOtherThanBody(
            string routeTemplate,
            string actionFixtureName,
            string expectedIn,
            string expectedType)
        {
            var subject = Subject(setupApis: apis => apis.Add("POST", routeTemplate, actionFixtureName));

            var swagger = subject.GetSwagger("v1");

            var param = swagger.Paths["/" + routeTemplate].Post.Parameters.First();
            Assert.IsAssignableFrom<NonBodyParameter>(param);
            var nonBodyParam = param as NonBodyParameter;
            Assert.Equal(expectedIn, nonBodyParam.In);
            Assert.Equal(expectedType, nonBodyParam.Type);
        }

        [Fact]
        public void GetSwagger_GeneratesQueryParams_ForParamsWithNoBinding()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(FakeController.AcceptsString))
                .Add("POST", "collection", nameof(FakeController.AcceptsComplexType)));

            var swagger = subject.GetSwagger("v1");

            var getParam = swagger.Paths["/collection"].Get.Parameters.First();
            Assert.Equal("query", getParam.In);
            // Multiple post parameters as ApiExplorer flattens out the complex type
            var postParams = swagger.Paths["/collection"].Post.Parameters;
            Assert.All(postParams, (p) => Assert.Equal("query", p.In));
        }

        [Theory]
        [InlineData("collection/{param}")]
        [InlineData("collection/{param?}")]
        public void GetSwagger_SetsParameterRequired_ForRequiredAndOptionalRouteParams(string routeTemplate)
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", routeTemplate, nameof(FakeController.AcceptsStringFromRoute)));

            var swagger = subject.GetSwagger("v1");

            var param = swagger.Paths["/collection/{param}"].Get.Parameters.First();
            Assert.True(param.Required);
        }

        [Theory]
        [InlineData(nameof(FakeController.AcceptsDataAnnotatedParams), "stringWithNoAttributes", false)]
        [InlineData(nameof(FakeController.AcceptsDataAnnotatedParams), "stringWithRequired", true)]
        [InlineData(nameof(FakeController.AcceptsDataAnnotatedParams), "intWithRequired", true)]
        [InlineData(nameof(FakeController.AcceptsDataAnnotatedType), "StringWithNoAttributes", false)]
        [InlineData(nameof(FakeController.AcceptsDataAnnotatedType), "StringWithRequired", true)]
        [InlineData(nameof(FakeController.AcceptsDataAnnotatedType), "IntWithRequired", true)]
        [InlineData(nameof(FakeController.AcceptsBindingAnnotatedType), "StringWithNoAttributes", false)]
        [InlineData(nameof(FakeController.AcceptsBindingAnnotatedType), "StringWithBindRequired", true)]
        [InlineData(nameof(FakeController.AcceptsBindingAnnotatedType), "IntWithBindRequired", true)]
        public void GetSwagger_SetsParameterRequired_ForParametersWithRequiredOrBindRequiredAttribute(
            string actionFixtureName,
            string parameterName,
            bool expectedRequired)
        {
            var subject = Subject(setupApis: apis => apis.Add("GET", "collection", actionFixtureName));

            var swagger = subject.GetSwagger("v1");

            var parameter = swagger.Paths["/collection"].Get.Parameters.First(p => p.Name == parameterName);
            Assert.True(parameter.Required == expectedRequired, $"{parameterName}.required != {expectedRequired}");
        }

        [Fact]
        public void GetSwagger_SetsParameterTypeString_ForUnboundRouteParams()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection/{param}", nameof(FakeController.AcceptsNothing)));

            var swagger = subject.GetSwagger("v1");

            var param = swagger.Paths["/collection/{param}"].Get.Parameters.First();
            Assert.IsAssignableFrom<NonBodyParameter>(param);
            var nonBodyParam = param as NonBodyParameter;
            Assert.Equal("param", nonBodyParam.Name);
            Assert.Equal("path", nonBodyParam.In);
            Assert.Equal("string", nonBodyParam.Type);
        }

        [Fact]
        public void GetSwagger_SetsDefaultValue_ForOptionalTopLevelParams()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection/{param}", nameof(FakeController.AcceptsOptionalParameter)));

            var swagger = subject.GetSwagger("v1");

            var param = swagger.Paths["/collection/{param}"].Get.Parameters.First();
            Assert.IsAssignableFrom<NonBodyParameter>(param);
            var nonBodyParam = param as NonBodyParameter;
            Assert.Equal("foobar", nonBodyParam.Default);
        }

        [Fact]
        public void GetSwagger_IgnoresParameters_IfPartOfCancellationToken()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(FakeController.AcceptsCancellationToken)));

            var swagger = subject.GetSwagger("v1");

            var operation = swagger.Paths["/collection"].Get;
            Assert.Empty(operation.Parameters);
        }

        [Fact]
        public void GetSwagger_IgnoresParameters_IfDecoratedWithBindNever()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(FakeController.AcceptsBindingAnnotatedType)));

            var swagger = subject.GetSwagger("v1");

            var parameterNames = swagger.Paths["/collection"].Get
                .Parameters
                .Select(p => p.Name);
            Assert.DoesNotContain("PropertyWithBindNever", parameterNames);
        }

        [Fact]
        public void GetSwagger_DescribesParametersInCamelCase_IfSpecifiedBySettings()
        {
            var subject = Subject(
                setupApis: apis => apis.Add("GET", "collection", nameof(FakeController.AcceptsBindingAnnotatedType)),
                setupAction: c => c.DescribeAllParametersInCamelCase = true
            );

            var swagger = subject.GetSwagger("v1");

            var operation = swagger.Paths["/collection"].Get;
            Assert.Equal(3, operation.Parameters.Count);
            Assert.Equal("stringWithNoAttributes", operation.Parameters[0].Name);
            Assert.Equal("stringWithBindRequired", operation.Parameters[1].Name);
            Assert.Equal("intWithBindRequired", operation.Parameters[2].Name);
        }

        [Theory]
        [InlineData(nameof(FakeController.ReturnsVoid), "200", "Success", false)]
        [InlineData(nameof(FakeController.ReturnsEnumerable), "200", "Success", true)]
        [InlineData(nameof(FakeController.ReturnsComplexType), "200", "Success", true)]
        [InlineData(nameof(FakeController.ReturnsJObject), "200", "Success", true)]
        [InlineData(nameof(FakeController.ReturnsActionResult), "200", "Success", false)]
        public void GetSwagger_GeneratesResponsesFromReturnTypes_IfResponseTypeAttributesNotPresent(
            string actionFixtureName,
            string expectedStatusCode,
            string expectedDescriptions,
            bool expectASchema)
        {
            var subject = Subject(setupApis: apis =>
                apis.Add("GET", "collection", actionFixtureName));

            var swagger = subject.GetSwagger("v1");

            var responses = swagger.Paths["/collection"].Get.Responses;
            Assert.Equal(new[] { expectedStatusCode }, responses.Keys.ToArray());
            var response = responses[expectedStatusCode];
            Assert.Equal(expectedDescriptions, response.Description);
            if (expectASchema)
                Assert.NotNull(response.Schema);
            else
                Assert.Null(response.Schema);
        }

        [Fact]
        public void GetSwagger_GeneratesResponsesFromResponseTypeAttributes_IfResponseTypeAttributesPresent()
        {
            var subject = Subject(setupApis: apis =>
                apis.Add("GET", "collection", nameof(FakeController.AnnotatedWithResponseTypeAttributes)));

            var swagger = subject.GetSwagger("v1");

            var responses = swagger.Paths["/collection"].Get.Responses;
            Assert.Equal(new[] { "204", "400" }, responses.Keys.ToArray());
            var response1 = responses["204"];
            Assert.Equal("Success", response1.Description);
            Assert.Null(response1.Schema);
            var response2 = responses["400"];
            Assert.Equal("Bad Request", response2.Description);
            Assert.NotNull(response2.Schema);
        }

        [Fact]
        public void GetSwagger_GeneratesDefaultResponseFromResponseTypeAttributes()
        {
            var subject = Subject(setupApis: apis =>
            {
                apis.Add("GET", "collection", nameof(FakeController.AnnotatedWithResponseTypeAttributes));
                var operation = apis.ApiDescriptionGroups.Items[0].Items.First(f => f.RelativePath == "collection");
                operation.SupportedResponseTypes.Add(new ApiResponseTypeV2 { IsDefaultResponse = true });
            });

            var swagger = subject.GetSwagger("v1");

            var responses = swagger.Paths["/collection"].Get.Responses;
            Assert.Collection(
                responses,
                kvp =>
                {
                    Assert.Equal("204", kvp.Key);
                    var response = kvp.Value;
                    Assert.Equal("Success", response.Description);
                },
                kvp =>
                {
                    Assert.Equal("400", kvp.Key);
                    var response = kvp.Value;
                    Assert.Equal("Bad Request", response.Description);
                },
                kvp =>
                {
                    Assert.Equal("default", kvp.Key);
                    var response = kvp.Value;
                    Assert.Null(response.Description);
                });
        }

        [Fact]
        public void GetSwagger_SetsDeprecated_IfActionsMarkedObsolete()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(FakeController.MarkedObsolete)));

            var swagger = subject.GetSwagger("v1");

            var operation = swagger.Paths["/collection"].Get;
            Assert.True(operation.Deprecated);
        }

        [Fact]
        public void GetSwagger_GeneratesBasicAuthSecurityDefinition_IfSpecifiedBySettings()
        {
            var subject = Subject(setupAction: c =>
                c.SecurityDefinitions.Add("basic", new BasicAuthScheme
                {
                    Type = "basic",
                    Description = "Basic HTTP Authentication"
                }));

            var swagger = subject.GetSwagger("v1");

            Assert.Contains("basic", swagger.SecurityDefinitions.Keys);
            var scheme = swagger.SecurityDefinitions["basic"];
            Assert.Equal("basic", scheme.Type);
            Assert.Equal("Basic HTTP Authentication", scheme.Description);
        }

        [Fact]
        public void GetSwagger_GeneratesApiKeySecurityDefinition_IfSpecifiedBySettings()
        {
            var subject = Subject(setupAction: c =>
                c.SecurityDefinitions.Add("apiKey", new ApiKeyScheme
                {
                    Type = "apiKey",
                    Description = "API Key Authentication",
                    Name = "apiKey",
                    In = "header"
                }));

            var swagger = subject.GetSwagger("v1");

            Assert.Contains("apiKey", swagger.SecurityDefinitions.Keys);
            var scheme = swagger.SecurityDefinitions["apiKey"];
            Assert.IsAssignableFrom<ApiKeyScheme>(scheme);
            var apiKeyScheme = scheme as ApiKeyScheme;
            Assert.Equal("apiKey", apiKeyScheme.Type);
            Assert.Equal("API Key Authentication", apiKeyScheme.Description);
            Assert.Equal("apiKey", apiKeyScheme.Name);
            Assert.Equal("header", apiKeyScheme.In);
        }

        [Fact]
        public void GetSwagger_GeneratesOAuthSecurityDefinition_IfSpecifiedBySettings()
        {
            var subject = Subject(setupAction: c =>
                c.SecurityDefinitions.Add("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Description = "OAuth2 Authorization Code Grant",
                    Flow = "accessCode",
                    AuthorizationUrl = "https://tempuri.org/auth",
                    TokenUrl = "https://tempuri.org/token",
                    Scopes = new Dictionary<string, string>
                    {
                        { "read", "Read access to protected resources" },
                        { "write", "Write access to protected resources" }
                    }
                }));

            var swagger = subject.GetSwagger("v1");

            Assert.Contains("oauth2", swagger.SecurityDefinitions.Keys);
            var scheme = swagger.SecurityDefinitions["oauth2"];
            Assert.IsAssignableFrom<OAuth2Scheme>(scheme);
            var oAuth2Scheme = scheme as OAuth2Scheme;
            Assert.Equal("oauth2", oAuth2Scheme.Type);
            Assert.Equal("OAuth2 Authorization Code Grant", oAuth2Scheme.Description);
            Assert.Equal("accessCode", oAuth2Scheme.Flow);
            Assert.Equal("https://tempuri.org/auth", oAuth2Scheme.AuthorizationUrl);
            Assert.Equal("https://tempuri.org/token", oAuth2Scheme.TokenUrl);
            Assert.Equal(new[] { "read", "write" }, oAuth2Scheme.Scopes.Keys.ToArray());
            Assert.Equal("Read access to protected resources", oAuth2Scheme.Scopes["read"]);
            Assert.Equal("Write access to protected resources", oAuth2Scheme.Scopes["write"]);
        }

        [Fact]
        public void GetSwagger_IgnoresObsoleteActions_IfSpecifiedBySettings()
        {
            var subject = Subject(
                setupApis: apis =>
                {
                    apis.Add("GET", "collection1", nameof(FakeController.ReturnsEnumerable));
                    apis.Add("GET", "collection2", nameof(FakeController.MarkedObsolete));
                },
                setupAction: c => c.IgnoreObsoleteActions = true);

            var swagger = subject.GetSwagger("v1");

            Assert.Equal(new[] { "/collection1" }, swagger.Paths.Keys.ToArray());
        }

        [Fact]
        public void GetSwagger_TagsActions_AsSpecifiedBySettings()
        {
            var subject = Subject(
                setupApis: apis =>
                {
                    apis.Add("GET", "collection1", nameof(FakeController.ReturnsEnumerable));
                    apis.Add("GET", "collection2", nameof(FakeController.ReturnsInt));
                },
                setupAction: c => c.TagSelector = (apiDesc) => apiDesc.RelativePath);

            var swagger = subject.GetSwagger("v1");

            Assert.Equal(new[] { "collection1" }, swagger.Paths["/collection1"].Get.Tags);
            Assert.Equal(new[] { "collection2" }, swagger.Paths["/collection2"].Get.Tags);
        }

        [Fact]
        public void GetSwagger_OrdersActions_AsSpecifiedBySettings()
        {
            var subject = Subject(
                setupApis: apis =>
                {
                    apis.Add("GET", "B", nameof(FakeController.ReturnsVoid));
                    apis.Add("GET", "A", nameof(FakeController.ReturnsVoid));
                    apis.Add("GET", "F", nameof(FakeController.ReturnsVoid));
                    apis.Add("GET", "D", nameof(FakeController.ReturnsVoid));
                },
                setupAction: c =>
                {
                    c.SortKeySelector = (apiDesc) => apiDesc.RelativePath;
                });

            var swagger = subject.GetSwagger("v1");

            Assert.Equal(new[] { "/A", "/B", "/D", "/F" }, swagger.Paths.Keys.ToArray());
        }

        [Fact]
        public void GetSwagger_ExecutesOperationFilters_IfSpecifiedBySettings()
        {
            var subject = Subject(
                setupApis: apis =>
                {
                    apis.Add("GET", "collection", nameof(FakeController.ReturnsEnumerable));
                },
                setupAction: c =>
                {
                    c.OperationFilters.Add(new VendorExtensionsOperationFilter());
                });

            var swagger = subject.GetSwagger("v1");

            var operation = swagger.Paths["/collection"].Get;
            Assert.NotEmpty(operation.Extensions);
        }

        [Fact]
        public void GetSwagger_ExecutesDocumentFilters_IfSpecifiedBySettings()
        {
            var subject = Subject(setupAction: opts =>
                opts.DocumentFilters.Add(new VendorExtensionsDocumentFilter()));

            var swagger = subject.GetSwagger("v1");

            Assert.NotEmpty(swagger.Extensions);
        }

        [Fact]
        public void GetSwagger_HandlesUnboundRouteParams()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "{version}/collection", nameof(FakeController.AcceptsNothing)));

            var swagger = subject.GetSwagger("v1");

            var param = swagger.Paths["/{version}/collection"].Get.Parameters.First();
            Assert.Equal("version", param.Name);
            Assert.Equal(true, param.Required);
        }


        [Fact]
        public void GetSwagger_ThrowsInformativeException_IfActionsHaveNoHttpBinding()
        {
            var subject = Subject(setupApis: apis => apis
                .Add(null, "collection", nameof(FakeController.AcceptsNothing)));

            var exception = Assert.Throws<NotSupportedException>(() => subject.GetSwagger("v1"));
            Assert.Equal(
                "Ambiguous HTTP method for action - Swashbuckle.AspNetCore.SwaggerGen.Test.FakeController.AcceptsNothing (Swashbuckle.AspNetCore.SwaggerGen.Test). " +
                "Actions require an explicit HttpMethod binding for Swagger 2.0",
                exception.Message);
        }

        [Fact]
        public void GetSwagger_ThrowsInformativeException_IfActionsHaveConflictingHttpMethodAndPath()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(FakeController.AcceptsNothing))
                .Add("GET", "collection", nameof(FakeController.AcceptsStringFromQuery))
            );

            var exception = Assert.Throws<NotSupportedException>(() => subject.GetSwagger("v1"));
            Assert.Equal(
                "HTTP method \"GET\" & path \"collection\" overloaded by actions - " +
                "Swashbuckle.AspNetCore.SwaggerGen.Test.FakeController.AcceptsNothing (Swashbuckle.AspNetCore.SwaggerGen.Test)," +
                "Swashbuckle.AspNetCore.SwaggerGen.Test.FakeController.AcceptsStringFromQuery (Swashbuckle.AspNetCore.SwaggerGen.Test). " +
                "Actions require unique method/path combination for Swagger 2.0. Use ConflictingActionsResolver as a workaround",
                exception.Message);
        }

        [Fact]
        public void GetSwagger_MergesActionsWithConflictingHttpMethodAndPath_IfResolverIsProvidedWithSettings()
        {
            var subject = Subject(setupApis:
                apis => apis
                    .Add("GET", "collection", nameof(FakeController.AcceptsNothing))
                    .Add("GET", "collection", nameof(FakeController.AcceptsStringFromQuery)),
                setupAction: c => { c.ConflictingActionsResolver = (apiDescriptions) => apiDescriptions.First(); }
            );

            var swagger = subject.GetSwagger("v1");

            var operation = swagger.Paths["/collection"].Get;
            Assert.Empty(operation.Parameters); // first one has no parameters
        }

        private SwaggerGenerator Subject(
            Action<FakeApiDescriptionGroupCollectionProvider> setupApis = null,
            Action<SwaggerGeneratorOptions> setupAction = null)
        {
            var apiDescriptionsProvider = new FakeApiDescriptionGroupCollectionProvider();
            setupApis?.Invoke(apiDescriptionsProvider);

            var options = new SwaggerGeneratorOptions();
            options.SwaggerDocs.Add("v1", new Info { Title = "API", Version = "v1" });

            setupAction?.Invoke(options);

            return new SwaggerGenerator(
                apiDescriptionsProvider,
                new SchemaRegistryFactory(new JsonSerializerSettings(), new SchemaRegistryOptions()),
                options
            );
        }

        private class ApiResponseTypeV2 : ApiResponseType
        {
            public bool IsDefaultResponse { get; set; }
        }
    }
}