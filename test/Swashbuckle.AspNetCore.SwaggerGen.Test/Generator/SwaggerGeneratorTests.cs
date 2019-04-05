using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Newtonsoft.Json;
using Xunit;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class SwaggerGeneratorTests
    {
        [Fact]
        public void GetSwagger_ThrowsException_IfDocumentNameIsUnknown()
        {
            var subject = Subject(setupAction: (c) => c.SwaggerDocs.Clear());

            Assert.Throws<UnknownSwaggerDocument>(() => subject.GetSwagger("v1"));
        }

        [Fact]
        public void GetSwagger_GeneratesSwagger_ForEachConfiguredDocument()
        {
            var v1Info = new OpenApiInfo { Version = "v2", Title = "API V2" };
            var v2Info = new OpenApiInfo { Version = "v1", Title = "API V1" };

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
                .Add("POST", "collection", nameof(FakeController.AcceptsComplexTypeFromBody))
                .Add("PUT", "collection/{id}", nameof(FakeController.AcceptsComplexTypeFromBody))
                .Add("DELETE", "collection/{id}", nameof(FakeController.ReturnsVoid))
                .Add("PATCH", "collection/{id}", nameof(FakeController.AcceptsComplexTypeFromBody))
            );

            var swagger = subject.GetSwagger("v1");

            Assert.Equal(new[] { OperationType.Get, OperationType.Post }, swagger.Paths["/collection"].Operations.Keys);
            Assert.Equal(new[] { OperationType.Put, OperationType.Delete, OperationType.Patch }, swagger.Paths["/collection/{id}"].Operations.Keys);
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
                "Actions require unique method/path combination for OpenAPI 3.0. Use ConflictingActionsResolver as a workaround",
                exception.Message);
        }

        [Fact]
        public void GetSwagger_MergesConflictingActions_AccordingToConfiguredStrategy()
        {
            var subject = Subject(setupApis:
                apis => apis
                    .Add("GET", "collection", nameof(FakeController.AcceptsNothing))
                    .Add("GET", "collection", nameof(FakeController.AcceptsStringFromQuery)),
                setupAction: c => { c.ConflictingActionsResolver = (apiDescriptions) => apiDescriptions.First(); }
            );

            var swagger = subject.GetSwagger("v1");

            var operation = swagger.Paths["/collection"].Operations[OperationType.Get];
            Assert.Empty(operation.Parameters); // first one has no parameters
        }

        [Fact]
        public void GetSwagger_IgnoresObsoleteActions_IfConfigFlagIsSet()
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
        public void GetSwagger_OrdersActions_AccordingToConfiguredStrategy()
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
        public void GetSwagger_TagsActions_AccordingToConfiguredStrategy()
        {
            var subject = Subject(
                setupApis: apis =>
                {
                    apis.Add("GET", "collection1", nameof(FakeController.ReturnsEnumerable));
                    apis.Add("GET", "collection2", nameof(FakeController.ReturnsInt));
                },
                setupAction: c => c.TagsSelector = (apiDesc) => new[] { apiDesc.RelativePath });

            var paths = subject.GetSwagger("v1").Paths;

            Assert.Equal(new[] { "collection1" }, paths["/collection1"].Operations[OperationType.Get].Tags.Select(t => t.Name));
            Assert.Equal(new[] { "collection2" }, paths["/collection2"].Operations[OperationType.Get].Tags.Select(t => t.Name));
        }

        [Fact]
        public void GetSwagger_SetsOperationIdToNull_ByDefault()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "resource", nameof(FakeController.AcceptsString)));

            var swagger = subject.GetSwagger("v1");

            Assert.Null(swagger.Paths["/resource"].Operations[OperationType.Get].OperationId);
        }

        [Fact]
        public void GetSwagger_SetsOperationIdToRouteName_IfProvidedWithHttpMethodAttribute()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "resource", nameof(FakeController.AnnotatedWithRouteName)));

            var swagger = subject.GetSwagger("v1");

            Assert.Equal("GetResource", swagger.Paths["/resource"].Operations[OperationType.Get].OperationId);
        }

        [Theory]
        [InlineData("resource/{param}", nameof(FakeController.AcceptsStringFromRoute), ParameterLocation.Path, "string")]
        [InlineData("resource", nameof(FakeController.AcceptsIntegerFromQuery), ParameterLocation.Query, "integer")]
        [InlineData("resource", nameof(FakeController.AcceptsStringFromHeader), ParameterLocation.Header, "string")]
        public void GetSwagger_GeneratesParameters_ForApiParametersThatAreNotBoundToBodyOrForm(
            string routeTemplate,
            string actionFixtureName,
            ParameterLocation expectedLocation,
            string expectedType)
        {
            var subject = Subject(setupApis: apis => apis.Add("POST", routeTemplate, actionFixtureName));

            var swagger = subject.GetSwagger("v1");

            var parameter = swagger.Paths["/" + routeTemplate].Operations[OperationType.Post].Parameters.First();
            Assert.Equal(expectedLocation, parameter.In);
            Assert.NotNull(parameter.Schema);
            Assert.Equal(expectedType, parameter.Schema.Type);
        }

        [Fact]
        public void GetSwagger_SetsParameterLocationToQuery_IfApiParameterHasNoExplicitBinding()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(FakeController.AcceptsString))
                .Add("POST", "collection", nameof(FakeController.AcceptsComplexType)));

            var swagger = subject.GetSwagger("v1");

            var getParameter = swagger.Paths["/collection"].Operations[OperationType.Get].Parameters.First();
            Assert.Equal(ParameterLocation.Query, getParameter.In);
            // Multiple post parameters as ApiExplorer flattens out the complex type
            var postParameters = swagger.Paths["/collection"].Operations[OperationType.Post].Parameters;
            Assert.All(postParameters, (p) => Assert.Equal(ParameterLocation.Query, p.In));
        }

        [Fact]
        public void GetSwagger_SetsParameterTypeString_IfApiParameterHasNoCorrespondingActionParameter()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection/{param}", nameof(FakeController.AcceptsNothing)));

            var swagger = subject.GetSwagger("v1");

            var parameter = swagger.Paths["/collection/{param}"].Operations[OperationType.Get].Parameters.First();
            Assert.Equal("param", parameter.Name);
            Assert.NotNull(parameter.Schema);
            Assert.Equal("string", parameter.Schema.Type);
        }

        [Theory]
        [InlineData("collection/{param}")]
        [InlineData("collection/{param?}")]
        public void GetSwagger_SetsParameterRequired_IfApiParameterIsFromRoute(string routeTemplate)
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", routeTemplate, nameof(FakeController.AcceptsStringFromRoute)));

            var swagger = subject.GetSwagger("v1");

            var parameter = swagger.Paths["/collection/{param}"].Operations[OperationType.Get].Parameters.First();
            Assert.True(parameter.Required);
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
        public void GetSwagger_SetsParameterRequired_IfApiParameterHasRequiredOrBindRequiredAttribute(
            string actionFixtureName,
            string parameterName,
            bool expectedRequired)
        {
            var subject = Subject(setupApis: apis => apis.Add("GET", "collection", actionFixtureName));

            var swagger = subject.GetSwagger("v1");

            var parameter = swagger.Paths["/collection"].Operations[OperationType.Get].Parameters.First(p => p.Name == parameterName);
            Assert.True(parameter.Required == expectedRequired, $"{parameterName}.required != {expectedRequired}");
        }

        [Theory]
        [InlineData(nameof(FakeController.AcceptsOptionalParameter), "param", "foobar")]
        [InlineData(nameof(FakeController.AcceptsOptionalJsonConvertedEnum), "param", "Value1")]
        [InlineData(nameof(FakeController.AcceptsDataAnnotatedType), "StringWithDefaultValue", "foobar")]
        public void GetSwagger_SetsDefaultValue_IfApiParameterIsOptionalOrHasDefaultValueAttribute(
            string actionFixtureName,
            string parameterName,
            string expectedDefaultValue)
        {
            var subject = Subject(setupApis: apis => apis.Add("GET", "collection", actionFixtureName));

            var swagger = subject.GetSwagger("v1");

            var parameter = swagger.Paths["/collection"].Operations[OperationType.Get].Parameters.First(p => p.Name == parameterName);
            Assert.IsType<OpenApiString>(parameter.Schema.Default);
            Assert.Equal(expectedDefaultValue, ((OpenApiString)parameter.Schema.Default).Value);
        }

        [Fact]
        public void GetSwagger_IgnoresParameters_IfApiParameterIsCancellationToken()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(FakeController.AcceptsCancellationToken)));

            var swagger = subject.GetSwagger("v1");

            var operation = swagger.Paths["/collection"].Operations[OperationType.Get];
            Assert.Empty(operation.Parameters);
        }

        [Fact]
        public void GetSwagger_IgnoresParameters_IfApiParameterHasBindNeverAttribute()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(FakeController.AcceptsBindingAnnotatedType)));

            var swagger = subject.GetSwagger("v1");

            var parameterNames = swagger.Paths["/collection"].Operations[OperationType.Get]
                .Parameters
                .Select(p => p.Name);
            Assert.DoesNotContain("PropertyWithBindNever", parameterNames);
        }

        [Fact]
        public void GetSwagger_HonorsNullableType()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("POST", "collection", nameof(FakeController.AcceptsComplexTypeFromBody)));

            var swagger = subject.GetSwagger("v1");

            var schema = swagger.Components.Schemas.FirstOrDefault(e => e.Key == "ComplexType");
            var property1 = schema.Value.Properties.SingleOrDefault(e => e.Key == "Property1");
            Assert.False(property1.Value.Nullable);

            var property2 = schema.Value.Properties.SingleOrDefault(e => e.Key == "Property2");
            Assert.False(property2.Value.Nullable);

            var property3 = schema.Value.Properties.SingleOrDefault(e => e.Key == "Property3");
            Assert.False(property3.Value.Nullable);

            var property4 = schema.Value.Properties.SingleOrDefault(e => e.Key == "Property4");
            Assert.False(property4.Value.Nullable);

            var property5 = schema.Value.Properties.SingleOrDefault(e => e.Key == "Property5");
            Assert.False(property5.Value.Nullable);

            var property6 = schema.Value.Properties.SingleOrDefault(e => e.Key == "Property6");
            Assert.False(property6.Value.Nullable);

            var property7 = schema.Value.Properties.SingleOrDefault(e => e.Key == "Property7");
            Assert.True(property7.Value.Nullable);

            var property8 = schema.Value.Properties.SingleOrDefault(e => e.Key == "Property8");
            Assert.True(property8.Value.Nullable);

            var property9 = schema.Value.Properties.SingleOrDefault(e => e.Key == "Property9");
            Assert.True(property9.Value.Nullable);
        }

        [Fact]
        public void GetSwagger_NamesAllParametersInCamelCase_IfSpecifiedBySettings()
        {
            var subject = Subject(
                setupApis: apis => apis.Add("GET", "collection", nameof(FakeController.AcceptsBindingAnnotatedType)),
                setupAction: c => c.DescribeAllParametersInCamelCase = true
            );

            var swagger = subject.GetSwagger("v1");

            var operation = swagger.Paths["/collection"].Operations[OperationType.Get];
            Assert.Equal(3, operation.Parameters.Count);
            Assert.Equal("stringWithNoAttributes", operation.Parameters[0].Name);
            Assert.Equal("stringWithBindRequired", operation.Parameters[1].Name);
            Assert.Equal("intWithBindRequired", operation.Parameters[2].Name);
        }

        [Fact]
        public void GetSwagger_GeneratesRequestBody_ForFirstApiParameterThatIsBoundToBody()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("POST", "collection", nameof(FakeController.AcceptsComplexTypeFromBody)));

            var swagger = subject.GetSwagger("v1");

            var requestBody = swagger.Paths["/collection"].Operations[OperationType.Post].RequestBody;
            Assert.NotNull(requestBody);
            Assert.False(requestBody.Required);
            Assert.Equal(new[] { "application/json", "text/json", "application/*+json" }, requestBody.Content.Keys);
            Assert.All(requestBody.Content.Values, mediaType =>
            {
                Assert.NotNull(mediaType.Schema);
                Assert.NotNull(mediaType.Schema.Reference);
                Assert.Equal("ComplexType", mediaType.Schema.Reference.Id);
            });
        }

        [Fact]
        public void GetSwagger_GeneratesRequestBody_ForFirstApiParameterThatIsBoundToBody_ThatIsRequired()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("POST", "collection", nameof(FakeController.AcceptsComplexTypeFromBodyThatIsRequired)));

            var swagger = subject.GetSwagger("v1");

            var requestBody = swagger.Paths["/collection"].Operations[OperationType.Post].RequestBody;
            Assert.NotNull(requestBody);
            Assert.True(requestBody.Required);
            Assert.Equal(new[] { "application/json", "text/json", "application/*+json" }, requestBody.Content.Keys);
            Assert.All(requestBody.Content.Values, mediaType =>
            {
                Assert.NotNull(mediaType.Schema);
                Assert.NotNull(mediaType.Schema.Reference);
                Assert.Equal("ComplexType", mediaType.Schema.Reference.Id);
            });
        }

        [Fact]
        public void GetSwagger_GeneratesRequestBody_ForApiParametersThatAreBoundToForm()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("POST", "collection", nameof(FakeController.AcceptsComplexTypeFromForm)));

            var swagger = subject.GetSwagger("v1");

            var requestBody = swagger.Paths["/collection"].Operations[OperationType.Post].RequestBody;
            Assert.NotNull(requestBody);
            Assert.Equal(new[] { "multipart/form-data" }, requestBody.Content.Keys);
            Assert.All(requestBody.Content.Values, mediaType =>
            {
                Assert.NotNull(mediaType.Schema);
                Assert.Equal(8, mediaType.Schema.Properties.Count);
                Assert.NotNull(mediaType.Encoding);
            });
        }

        [Fact]
        public void GetSwagger_SetsRequestBodyContentTypesFromConsumesAttribute_IfPresent()
        {
            var subject = Subject(setupApis: apis =>
                apis.Add("POST", "resource", nameof(FakeController.AnnotatedWithConsumes)));

            var swagger = subject.GetSwagger("v1");

            var operation = swagger.Paths["/resource"].Operations[OperationType.Post];
            Assert.Equal(new[] { "application/xml" }, operation.RequestBody.Content.Keys);
        }

        [Theory]
        [InlineData(nameof(FakeController.ReturnsVoid), "200", "Success", new string[] { })]
        [InlineData(nameof(FakeController.ReturnsEnumerable), "200", "Success", new string[] { "application/json", "text/json" })]
        [InlineData(nameof(FakeController.ReturnsComplexType), "200", "Success", new string[] { "application/json", "text/json" })]
        [InlineData(nameof(FakeController.ReturnsJObject), "200", "Success", new string[] { "application/json", "text/json" })]
        [InlineData(nameof(FakeController.ReturnsActionResult), "200", "Success", new string[] { })]
        [InlineData(nameof(FakeController.AnnotatedWithWithNotModifiedResponseTypeAttributes), "304", "Not Modified", new string[] { })]
        public void GetSwagger_GeneratesResponses_ForSupportedApiResponseTypes(
            string actionFixtureName,
            string expectedStatusCode,
            string expectedDescriptions,
            string[] expectedContentTypes)
        {
            var subject = Subject(setupApis: apis =>
                apis.Add("GET", "collection", actionFixtureName));

            var swagger = subject.GetSwagger("v1");

            var responses = swagger.Paths["/collection"].Operations[OperationType.Get].Responses;
            Assert.Equal(new[] { expectedStatusCode }, responses.Keys.ToArray());
            var response = responses[expectedStatusCode];
            Assert.Equal(expectedDescriptions, response.Description);
            Assert.Equal(expectedContentTypes, response.Content.Keys);
        }

        [Fact]
        public void GetSwagger_GeneratesResponsesFromResponseTypeAttributes_IfPresent()
        {
            var subject = Subject(setupApis: apis =>
                apis.Add("GET", "collection", nameof(FakeController.AnnotatedWithResponseTypeAttributes)));

            var swagger = subject.GetSwagger("v1");

            var responses = swagger.Paths["/collection"].Operations[OperationType.Get].Responses;
            Assert.Equal(new[] { "204", "400" }, responses.Keys.ToArray());
            Assert.Equal("Success", responses["204"].Description);
            Assert.Empty(responses["204"].Content);
            Assert.Equal("Bad Request", responses["400"].Description);
            Assert.NotEmpty(responses["400"].Content);
        }

        [Fact]
        public void GetSwagger_SetsResponseContentTypesFromProducesAttribute_IfPresent()
        {
            var subject = Subject(setupApis: apis =>
                apis.Add("GET", "resource", nameof(FakeController.AnnotatedWithProduces)));

            var swagger = subject.GetSwagger("v1");

            var operation = swagger.Paths["/resource"].Operations[OperationType.Get];
            Assert.Equal(new[] { "application/xml" }, operation.Responses["200"].Content.Keys);
        }

        [Fact]
        public void GetSwagger_HonorsDefaultResponseFlagOnApiResponseType_IfPresent()
        {
            var subject = Subject(setupApis: apis =>
            {
                apis.Add("GET", "collection", nameof(FakeController.AnnotatedWithResponseTypeAttributes));
                var operation = apis.ApiDescriptionGroups.Items[0].Items.First(f => f.RelativePath == "collection");
                operation.SupportedResponseTypes.Add(new ApiResponseTypeV2 { IsDefaultResponse = true });
            });

            var swagger = subject.GetSwagger("v1");

            var responses = swagger.Paths["/collection"].Operations[OperationType.Get].Responses;
            Assert.Equal(new[] { "204", "400", "default" }, responses.Keys);
            Assert.Equal("Unexpected Error", responses["default"].Description);
        }

        [Fact]
        public void GetSwagger_SetsDeprecatedFlag_IfActionsMarkedObsolete()
        {
            var subject = Subject(setupApis: apis => apis
                .Add("GET", "collection", nameof(FakeController.MarkedObsolete)));

            var swagger = subject.GetSwagger("v1");

            var operation = swagger.Paths["/collection"].Operations[OperationType.Get];
            Assert.True(operation.Deprecated);
        }

        [Fact]
        public void GetSwagger_IncludesSecuritySchemes_IfConfigured()
        {
            var subject = Subject(setupAction: c =>
                c.SecuritySchemes.Add("basic", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic"
                }));

            var swagger = subject.GetSwagger("v1");

            Assert.Equal(new[] { "basic" }, swagger.Components.SecuritySchemes.Keys);
        }

        [Fact]
        public void GetSwagger_ExecutesOperationFilters_IfConfigured()
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

            var operation = swagger.Paths["/collection"].Operations[OperationType.Get];
            Assert.NotEmpty(operation.Extensions);
        }

        [Fact]
        public void GetSwagger_ExecutesDocumentFilters_IfConfigured()
        {
            var subject = Subject(setupAction: opts =>
                opts.DocumentFilters.Add(new VendorExtensionsDocumentFilter()));

            var swagger = subject.GetSwagger("v1");

            Assert.NotEmpty(swagger.Extensions);
        }

        private SwaggerGenerator Subject(
            Action<FakeApiDescriptionGroupCollectionProvider> setupApis = null,
            Action<SwaggerGeneratorOptions> setupAction = null)
        {
            var apiDescriptionsProvider = new FakeApiDescriptionGroupCollectionProvider();
            setupApis?.Invoke(apiDescriptionsProvider);

            var options = new SwaggerGeneratorOptions();
            options.SwaggerDocs.Add("v1", new OpenApiInfo { Title = "API", Version = "v1" });

            setupAction?.Invoke(options);

            return new SwaggerGenerator(
                apiDescriptionsProvider,
                new SchemaGenerator(new JsonSerializerSettings(), new SchemaGeneratorOptions()),
                options
            );
        }

        private class ApiResponseTypeV2 : ApiResponseType
        {
            public bool IsDefaultResponse { get; set; }
        }
    }
}