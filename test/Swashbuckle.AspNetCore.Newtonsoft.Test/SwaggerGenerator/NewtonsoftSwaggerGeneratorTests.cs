using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerGen.Test;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

public class NewtonsoftSwaggerGeneratorTests
{
    [Theory]
    [InlineData(nameof(FakeControllerWithAsyncEnumerable.ReturnsAsyncEnumerable))]
    [InlineData(nameof(FakeControllerWithAsyncEnumerable.ReturnsTypeImplementingAsyncEnumerable))]
    public void GetSwagger_GeneratesArrayResponseSchema_IfActionReturnsAsyncEnumerable(string actionName)
    {
        var document = Subject(actionName).GetSwagger("v1");

        var schema = document.Paths["/resource"].Operations[HttpMethod.Get].Responses["200"].Content["application/json"].Schema;
        Assert.Equal(JsonSchemaTypes.Array, schema.Type);
        var itemsSchema = Assert.IsType<OpenApiSchemaReference>(schema.Items);
        Assert.Equal("ComplexType", itemsSchema.Reference.Id);
        Assert.DoesNotContain("ComplexTypeIAsyncEnumerable", document.Components.Schemas.Keys);
        Assert.DoesNotContain(nameof(AsyncEnumerableOfComplexType), document.Components.Schemas.Keys);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GetSwagger_GeneratesArrayResponseSchema_IfActionReturnsAsyncEnumerableOfItself(bool hasRequestBody)
    {
        // With a request body, the type's own schema is registered before the response schema is generated
        ApiParameterDescription[] parameterDescriptions = hasRequestBody
            ? [new ApiParameterDescription { Name = "body", Source = BindingSource.Body }]
            : null;

        var document = Subject(
            nameof(FakeControllerWithAsyncEnumerable.ReturnsAsyncEnumerableOfSelf),
            httpMethod: "POST",
            parameterDescriptions: parameterDescriptions).GetSwagger("v1");

        var schema = document.Paths["/resource"].Operations[HttpMethod.Post].Responses["200"].Content["application/json"].Schema;
        Assert.Equal(JsonSchemaTypes.Array, schema.Type);
        var itemsSchema = Assert.IsType<OpenApiSchemaReference>(schema.Items);
        Assert.Equal(nameof(AsyncEnumerableOfSelf), itemsSchema.Reference.Id);

        var itemSchema = document.Components.Schemas[nameof(AsyncEnumerableOfSelf)];
        Assert.Equal(JsonSchemaTypes.Object, itemSchema.Type);
        Assert.Equal("Id", Assert.Single(itemSchema.Properties).Key);
    }

    [Theory]
    [InlineData(nameof(FakeControllerWithAsyncEnumerable.ReturnsAsyncEnumerable), typeof(IAsyncEnumerable<ComplexType>))]
    [InlineData(nameof(FakeControllerWithAsyncEnumerable.ReturnsTypeImplementingAsyncEnumerable), typeof(AsyncEnumerableOfComplexType))]
    [InlineData(nameof(FakeControllerWithAsyncEnumerable.ReturnsAsyncEnumerableOfSelf), typeof(AsyncEnumerableOfSelf))]
    public void GetSwagger_PassesResponseTypeToSchemaFilters_IfActionReturnsAsyncEnumerable(string actionName, Type responseType)
    {
        var schemaFilter = new RecordingSchemaFilter();

        var document = Subject(actionName, c => c.SchemaFilters.Add(schemaFilter)).GetSwagger("v1");

        var schema = document.Paths["/resource"].Operations[HttpMethod.Get].Responses["200"].Content["application/json"].Schema;
        Assert.Equal(JsonSchemaTypes.Array, schema.Type);
        Assert.Equal(responseType, schemaFilter.ContextTypes[schema]);
    }

    [Theory]
    [InlineData(nameof(FakeControllerWithAsyncEnumerable.ReturnsAsyncEnumerable), typeof(IAsyncEnumerable<ComplexType>))]
    [InlineData(nameof(FakeControllerWithAsyncEnumerable.ReturnsAsyncEnumerable), typeof(IAsyncEnumerable<>))]
    [InlineData(nameof(FakeControllerWithAsyncEnumerable.ReturnsTypeImplementingAsyncEnumerable), typeof(AsyncEnumerableOfComplexType))]
    [InlineData(nameof(FakeControllerWithAsyncEnumerable.ReturnsAsyncEnumerableOfSelf), typeof(AsyncEnumerableOfSelf))]
    public void GetSwagger_UsesCustomTypeMapping_IfActionReturnsAsyncEnumerable(string actionName, Type mappingType)
    {
        var document = Subject(
            actionName,
            c => c.CustomTypeMappings.Add(mappingType, () => new OpenApiSchema { Type = JsonSchemaTypes.String })).GetSwagger("v1");

        var schema = document.Paths["/resource"].Operations[HttpMethod.Get].Responses["200"].Content["application/json"].Schema;
        Assert.Equal(JsonSchemaTypes.String, schema.Type);
        Assert.DoesNotContain("ComplexType", document.Components.Schemas.Keys);
    }

    [Fact]
    public void GetSwagger_KeepsObjectSchema_IfResponsePropertyIsAsyncEnumerable()
    {
        var document = Subject(nameof(FakeControllerWithAsyncEnumerable.ReturnsTypeWithAsyncEnumerableProperty)).GetSwagger("v1");

        var schema = document.Paths["/resource"].Operations[HttpMethod.Get].Responses["200"].Content["application/json"].Schema;
        var reference = Assert.IsType<OpenApiSchemaReference>(schema);
        Assert.Equal(nameof(TypeWithAsyncEnumerableProperty), reference.Reference.Id);

        var propertySchema = Assert.IsType<OpenApiSchemaReference>(document.Components.Schemas[nameof(TypeWithAsyncEnumerableProperty)].Properties["Results"]);
        Assert.Equal("ComplexTypeIAsyncEnumerable", propertySchema.Reference.Id);
        Assert.Equal(JsonSchemaTypes.Object, document.Components.Schemas["ComplexTypeIAsyncEnumerable"].Type);
    }

    private static SwaggerGenerator Subject(
        string actionName,
        Action<SchemaGeneratorOptions> configureGenerator = null,
        string httpMethod = "GET",
        IEnumerable<ApiParameterDescription> parameterDescriptions = null)
    {
        var apiDescription = ApiDescriptionFactory.Create<FakeControllerWithAsyncEnumerable>(
            _ => actionName,
            groupName: "v1",
            httpMethod: httpMethod,
            relativePath: "resource",
            parameterDescriptions: parameterDescriptions,
            supportedResponseTypes:
            [
                new ApiResponseType
                {
                    ApiResponseFormats = [new ApiResponseFormat { MediaType = "application/json" }],
                    StatusCode = 200,
                },
            ]);

        var options = new SwaggerGeneratorOptions
        {
            SwaggerDocs = new Dictionary<string, OpenApiInfo>
            {
                ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" }
            }
        };

        var generatorOptions = new SchemaGeneratorOptions();
        configureGenerator?.Invoke(generatorOptions);

        return new SwaggerGenerator(
            options,
            new FakeApiDescriptionGroupCollectionProvider([apiDescription]),
            new SchemaGenerator(generatorOptions, new NewtonsoftDataContractResolver(new JsonSerializerSettings())));
    }
}
