using System;
using System.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;
using Swashbuckle.Swagger.Generator;
using Swashbuckle.Swagger.Application;
using Swashbuckle.Swagger.Fixtures.ApiDescriptions;
using Swashbuckle.Swagger.Fixtures.Extensions;

namespace Swashbuckle.Swagger
{
    public class SwaggerGeneratorTests
    {
        [Fact]
        public void GetSwagger_ReturnsDefaultInfoVersionAndTitle()
        {
            var swagger = Subject().GetSwagger("https://tempuri.org", "v1");

            Assert.Equal("v1", swagger.info.version);
            Assert.Equal("API V1", swagger.info.title);
        }

        [Theory]
        [InlineData("http://tempuri.org:8080/foobar", "tempuri.org:8080", "/foobar", new[] { "http" })]
        [InlineData("https://tempuri.org:443", "tempuri.org:443", null, new[] { "https" })]
        public void GetSwagger_ReturnsHostBasePathAndSchemes_FromRootUrl(
            string rootUrl,
            string expectedHost,
            string expectedBasePath,
            string[] expectedSchemes)
        {
            var swagger = Subject().GetSwagger(rootUrl, "v1");

            Assert.Equal(expectedHost, swagger.host);
            Assert.Equal(expectedBasePath, swagger.basePath);
            Assert.Equal(expectedSchemes, swagger.schemes.ToArray());
        }

        [Fact]
        public void GetSwagger_ReturnsPathItem_PerRelativePathSansQueryString()
        {
            var swaggerGenerator = Subject(setupApis: apis => apis
                .Add("GET", "collection1", nameof(ActionFixtures.ReturnsEnumerable))
                .Add("GET", "collection1/{id}", nameof(ActionFixtures.ReturnsComplexType))
                .Add("GET", "collection2", nameof(ActionFixtures.AcceptsStringFromQuery))
                .Add("PUT", "collection2", nameof(ActionFixtures.ReturnsVoid))
                .Add("GET", "collection2/{id}", nameof(ActionFixtures.ReturnsComplexType))
            );

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            Assert.Equal(new[]
                {
                    "/collection1",
                    "/collection1/{id}",
                    "/collection2",
                    "/collection2/{id}"
                },
                swagger.paths.Keys.ToArray());
        }

        [Fact]
        public void GetSwagger_GeneratesOperation_PerHttpMethodPerRelativePathSansQueryString()
        {
            var swaggerGenerator = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(ActionFixtures.ReturnsEnumerable))
                .Add("PUT", "collection/{id}", nameof(ActionFixtures.ReturnsVoid))
                .Add("POST", "collection", nameof(ActionFixtures.ReturnsInt))
                .Add("DELETE", "collection/{id}", nameof(ActionFixtures.ReturnsVoid))
                .Add("PATCH", "collection/{id}", nameof(ActionFixtures.ReturnsVoid))
                // TODO: OPTIONS & HEAD
            );

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            // GET collection
            var operation = swagger.paths["/collection"].get;
            Assert.NotNull(operation);
            Assert.Equal("ReturnsEnumerable", operation.operationId);
            Assert.Equal(new[] { "application/json", "text/json" }, operation.produces.ToArray());
            // PUT collection/{id}
            operation = swagger.paths["/collection/{id}"].put;
            Assert.NotNull(operation);
            Assert.Equal("ReturnsVoid", operation.operationId);
            Assert.Empty(operation.produces.ToArray());
            // POST collection
            operation = swagger.paths["/collection"].post;
            Assert.NotNull(operation);
            Assert.Equal("ReturnsInt", operation.operationId);
            Assert.Equal(new[] { "application/json", "text/json" }, operation.produces.ToArray());
            // DELETE collection/{id}
            operation = swagger.paths["/collection/{id}"].delete;
            Assert.NotNull(operation);
            Assert.Equal("ReturnsVoid", operation.operationId);
            Assert.Empty(operation.produces.ToArray());
            // PATCH collection
            operation = swagger.paths["/collection/{id}"].patch;
            Assert.NotNull(operation);
            Assert.Equal("ReturnsVoid", operation.operationId);
            Assert.Empty(operation.produces.ToArray());
        }

        [Fact]
        public void GetSwagger_SetsParametersToNull_ForParameterlessActions()
        {
            var swaggerGenerator = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(ActionFixtures.AcceptsNothing)));

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            var operation = swagger.paths["/collection"].get;
            Assert.Null(operation.parameters);
        }

        [Theory]
        [InlineData("collection/{param}", nameof(ActionFixtures.AcceptsStringFromRoute), "path")]
        [InlineData("collection", nameof(ActionFixtures.AcceptsStringFromQuery), "query")]
        [InlineData("collection", nameof(ActionFixtures.AcceptsStringFromHeader), "header")]
        public void GetSwagger_SetsParameterIn_AccordingToParamBindingAttribute(
            string routeTemplate,
            string actionFixtureName,
            string expectedIn )
        {
            var swaggerGenerator = Subject(setupApis: apis => apis.Add("GET", routeTemplate, actionFixtureName));

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            var param = swagger.paths["/" + routeTemplate].get.parameters.First();
            Assert.Equal("param", param.name);
            Assert.Equal(expectedIn, param.@in);
            Assert.Equal("string", param.type);
        }

        [Fact]
        public void GetSwagger_SetsParameterSchema_ForParamsWithFromBodyAttribute()
        {
            var swaggerGenerator = Subject(setupApis: apis => apis
                .Add("POST", "collection", nameof(ActionFixtures.AcceptsComplexTypeFromBody)));

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            var param = swagger.paths["/collection"].post.parameters.First();
            Assert.Equal("param", param.name);
            Assert.Equal("body", param.@in);
            Assert.NotNull(param.schema);
            Assert.Equal("#/definitions/ComplexType", param.schema.@ref);
            Assert.Contains("ComplexType", swagger.definitions.Keys);
        }


        [Theory]
        [InlineData("collection/{param}", true)]
        [InlineData("collection/{param?}", false)]
        public void GetSwagger_SetsParameterRequired_ForRequiredRouteParams(
            string routeTemplate,
            bool expectedRequired)
        {
            var swaggerGenerator = Subject(setupApis: apis => apis
                .Add("GET", routeTemplate, nameof(ActionFixtures.AcceptsStringFromRoute)));

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            var param = swagger.paths["/collection/{param}"].get.parameters.First();
            Assert.Equal("param", param.name);
            Assert.Equal(expectedRequired, param.required);
        }

        [Theory]
        [InlineData("AcceptsStringFromQuery", false)]
        [InlineData("AcceptsRequiredStringFromQuery", true)]
        public void GetSwagger_SetsParameterRequired_ForNonRouteParamsMarkedRequired(
            string actionFixtureName,
            bool expectedRequired)
        {
            var swaggerGenerator = Subject(setupApis: apis => apis.Add("GET", "collection", actionFixtureName));

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            var param = swagger.paths["/collection"].get.parameters.First();
            Assert.Equal("param", param.name);
            Assert.Equal(expectedRequired, param.required);
        }

        [Fact]
        public void GetSwagger_GeneratesQueryParameters_ForComplexParamsWithFromQueryAttribute()
        {
            var swaggerGenerator = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(ActionFixtures.AcceptsComplexTypeFromQuery)));

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            var operation = swagger.paths["/collection"].get;
            Assert.Equal(3, operation.parameters.Count);
            Assert.Equal("Property1", operation.parameters[0].name);
            Assert.Equal("Property2", operation.parameters[1].name);
            Assert.Equal("Property3.Property1", operation.parameters[2].name);
        }

        [Theory]
        [InlineData(nameof(ActionFixtures.ReturnsVoid), "204", false)]
        [InlineData(nameof(ActionFixtures.ReturnsEnumerable), "200", true)]
        [InlineData(nameof(ActionFixtures.ReturnsComplexType), "200", true)]
        [InlineData(nameof(ActionFixtures.ReturnsActionResult), "200", true)]
        public void GetSwagger_SetsResponseStatusCodeAndSchema_AccordingToActionReturnType(
            string actionFixtureName,
            string expectedStatusCode,
            bool expectsSchema)
        {
            var swaggerGenerator = Subject(setupApis: apis =>
                apis.Add("GET", "collection", actionFixtureName));

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            var responses = swagger.paths["/collection"].get.responses;
            Assert.Equal(new[] { expectedStatusCode }, responses.Keys.ToArray());
            var response = responses[expectedStatusCode];
            if (expectsSchema)
                Assert.NotNull(response.schema);
            else
                Assert.Null(response.schema);
        }

        [Fact]
        public void GetSwagger_SetsDeprecated_ForActionsMarkedObsolete()
        {
            var swaggerGenerator = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(ActionFixtures.MarkedObsolete)));

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            var operation = swagger.paths["/collection"].get;
            Assert.Equal(true, operation.deprecated);
        }

        [Fact]
        public void GetSwagger_SupportsOptionToProvideAdditionalApiInfo()
        {
            var swaggerGenerator = Subject(
                setupOptions: opts => opts.SingleApiVersion("v3", "My API"));

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v3");

            Assert.NotNull(swagger.info);
            Assert.Equal("v3", swagger.info.version);
            Assert.Equal("My API", swagger.info.title);
            // TODO: More ...
        }

        [Fact]
        public void GetSwagger_SupportsOptionToDescribeMultipleApiVersions()
        {
            var swaggerGenerator = Subject(
                setupApis: apis =>
                {
                    apis.Add("GET", "v1/collection", nameof(ActionFixtures.ReturnsEnumerable));
                    apis.Add("GET", "v2/collection", nameof(ActionFixtures.ReturnsEnumerable));
                },
                setupOptions: opts =>
                {
                    opts.MultipleApiVersions(
                        (apiDesc, targetApiVersion) => apiDesc.RelativePath.StartsWith(targetApiVersion),
                        (vc) =>
                        {
                            vc.Version("v2", "API V2");
                            vc.Version("v1", "API V1");
                        });
                });

            var v2Swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v2");
            var v1Swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            Assert.Equal(new[] { "/v2/collection" }, v2Swagger.paths.Keys.ToArray());
            Assert.Equal(new[] { "/v1/collection" }, v1Swagger.paths.Keys.ToArray());
        }

        [Fact]
        public void GetSwagger_SupportsOptionToProvideExplicitHttpSchemes()
        {
            var swaggerGenerator = Subject(setupOptions: opts => opts.Schemes(new[] { "http", "https" }));

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            Assert.Equal(new[] { "http", "https" }, swagger.schemes.ToArray());
        }

        [Fact]
        public void GetSwagger_SupportsOptionToDefineBasicAuthScheme()
        {
            var swaggerGenerator = Subject(setupOptions: opts =>
                opts.BasicAuth("basic")
                    .Description("Basic HTTP Authentication"));

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            Assert.Contains("basic", swagger.securityDefinitions.Keys);
            var definition = swagger.securityDefinitions["basic"];
            Assert.Equal("basic", definition.type);
            Assert.Equal("Basic HTTP Authentication", definition.description);
        }

        [Fact]
        public void GetSwagger_SupportsOptionToDefineApiKeyScheme()
        {
            var swaggerGenerator = Subject(setupOptions: opts =>
                opts.ApiKey("apiKey")
                    .Description("API Key Authentication")
                    .Name("apiKey")
                    .In("header"));

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            Assert.Contains("apiKey", swagger.securityDefinitions.Keys);
            var definition = swagger.securityDefinitions["apiKey"];
            Assert.Equal("apiKey", definition.type);
            Assert.Equal("API Key Authentication", definition.description);
            Assert.Equal("apiKey", definition.name);
            Assert.Equal("header", definition.@in);
        }

        [Fact]
        public void GetSwagger_SupportsOptionToDefineOAuth2Scheme()
        {
            var swaggerGenerator = Subject(setupOptions: opts =>
                opts.OAuth2("oauth2")
                    .Description("OAuth2 Authorization Code Grant")
                    .Flow("accessCode")
                    .AuthorizationUrl("https://tempuri.org/auth")
                    .TokenUrl("https://tempuri.org/token")
                    .Scopes(s =>
                    {
                        s.Add("read", "Read access to protected resources");
                        s.Add("write", "Write access to protected resources");
                    }));

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            Assert.Contains("oauth2", swagger.securityDefinitions.Keys);
            var definition = swagger.securityDefinitions["oauth2"];
            Assert.Equal("oauth2", definition.type);
            Assert.Equal("OAuth2 Authorization Code Grant", definition.description);
            Assert.Equal("accessCode", definition.flow);
            Assert.Equal("https://tempuri.org/auth", definition.authorizationUrl);
            Assert.Equal("https://tempuri.org/token", definition.tokenUrl);
            Assert.Equal(new[] { "read", "write" }, definition.scopes.Keys.ToArray());
            Assert.Equal("Read access to protected resources", definition.scopes["read"]);
            Assert.Equal("Write access to protected resources", definition.scopes["write"]);
        }

        [Fact]
        public void GetSwagger_SupportsOptionToIgnoreObsoleteActions()
        {
            var swaggerGenerator = Subject(
                setupApis: apis =>
                {
                    apis.Add("GET", "collection1", nameof(ActionFixtures.ReturnsEnumerable));
                    apis.Add("GET", "collection2", nameof(ActionFixtures.MarkedObsolete));
                },
                setupOptions: opts => opts.IgnoreObsoleteActions());

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            Assert.Equal(new[] { "/collection1" }, swagger.paths.Keys.ToArray());
        }

        [Fact]
        public void GetSwagger_SupportsOptionToCustomizeGroupingOfActions()
        {
            var swaggerGenerator = Subject(
                setupApis: apis =>
                {
                    apis.Add("GET", "collection1", nameof(ActionFixtures.ReturnsEnumerable));
                    apis.Add("GET", "collection2", nameof(ActionFixtures.ReturnsInt));
                },
                setupOptions: opts => opts.GroupActionsBy((apiDesc) => apiDesc.RelativePath));

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            Assert.Equal(new[] { "collection1" }, swagger.paths["/collection1"].get.tags);
            Assert.Equal(new[] { "collection2" }, swagger.paths["/collection2"].get.tags);
        }

        [Fact]
        public void GetSwagger_SupportsOptionToCustomizeOderingOfActionGroups()
        {
            var swaggerGenerator = Subject(
                setupApis: apis =>
                {
                    apis.Add("GET", "A", nameof(ActionFixtures.ReturnsVoid));
                    apis.Add("GET", "B", nameof(ActionFixtures.ReturnsVoid));
                    apis.Add("GET", "F", nameof(ActionFixtures.ReturnsVoid));
                    apis.Add("GET", "D", nameof(ActionFixtures.ReturnsVoid));
                },
                setupOptions: opts =>
                {
                    opts.GroupActionsBy((apiDesc) => apiDesc.RelativePath);
                    opts.OrderActionGroupsBy(new DescendingAlphabeticComparer());
                });

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            Assert.Equal(new[] { "/F", "/D", "/B", "/A" }, swagger.paths.Keys.ToArray());
        }

        [Fact]
        public void GetSwagger_SupportsOptionToPostModifySwaggerDocument()
        {
            var swaggerGenerator = Subject(setupOptions: opts =>
                opts.DocumentFilter<VendorExtensionsDocumentFilter>());

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");
            
            Assert.NotEmpty(swagger.vendorExtensions);
        }

        [Fact]
        public void GetSwagger_SupportsOptionToResolveMultipleActionsWithSameHttpMethodAndPath()
        {
            var swaggerGenerator = Subject(
                setupApis: apis =>
                {
                    apis.Add("GET", "collection1", nameof(ActionFixtures.AcceptsNothing));
                    apis.Add("GET", "collection1", nameof(ActionFixtures.AcceptsStringFromQuery));
                },
                setupOptions: opts =>
                {
                    opts.ResolveConflictingActions(apiDescs => apiDescs.First());
                });

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            Assert.Equal(new[] { "/collection1" }, swagger.paths.Keys.ToArray());
        }

        [Fact]
        public void GetSwagger_HandlesUnboundRouteParams()
        {
            var swaggerGenerator = Subject(setupApis: apis => apis
                .Add("GET", "{version}/collection", nameof(ActionFixtures.AcceptsNothing)));

            var swagger = swaggerGenerator.GetSwagger("https://tempuri.org", "v1");

            var param = swagger.paths["/{version}/collection"].get.parameters.First();
            Assert.Equal("version", param.name);
            Assert.Equal(true, param.required);
        }

        private SwaggerGenerator Subject(
            Action<FakeApiDescriptionGroupCollectionProvider> setupApis = null,
            Action<SwaggerGeneratorOptionsBuilder> setupOptions = null)
        {
            var apiDescriptionsProvider = new FakeApiDescriptionGroupCollectionProvider();
            if (setupApis != null) setupApis(apiDescriptionsProvider);

            var optionsBuilder = new SwaggerGeneratorOptionsBuilder();
            if (setupOptions != null) setupOptions(optionsBuilder);

            return new SwaggerGenerator(
                apiDescriptionsProvider,
                () => new SchemaGenerator(new DefaultContractResolver()),
                optionsBuilder.Build()
            );
        }
    }
}