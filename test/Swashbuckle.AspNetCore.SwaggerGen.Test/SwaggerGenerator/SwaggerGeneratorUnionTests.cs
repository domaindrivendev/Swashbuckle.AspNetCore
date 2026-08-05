#if NET11_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class SwaggerGeneratorUnionTests
{
    public record Cat(string Name, int Lives);

    public record Dog(string Name, bool GoodBoy);

    public union Pet(Cat, Dog);

    public class UnionFakeController
    {
        public Pet GetPet() => throw new NotImplementedException();

        public Cat GetCat() => throw new NotImplementedException();
    }

    [Fact]
    public void GetSwagger_EmitsUnionResponseAsAnyOfOfReferences_AndDeduplicatesSharedCaseType()
    {
        // Endpoint 1 returns a union (Pet), endpoint 2 returns one of its cases (Cat) explicitly.
        var petApi = ApiDescriptionFactory.Create<UnionFakeController>(
            c => nameof(c.GetPet),
            groupName: "v1",
            httpMethod: "GET",
            relativePath: "pet",
            supportedResponseTypes:
            [
                new ApiResponseType
                {
                    ApiResponseFormats = [new ApiResponseFormat { MediaType = "application/json" }],
                    StatusCode = 200,
                    Type = typeof(Pet),
                },
            ]);

        var catApi = ApiDescriptionFactory.Create<UnionFakeController>(
            c => nameof(c.GetCat),
            groupName: "v1",
            httpMethod: "GET",
            relativePath: "cat",
            supportedResponseTypes:
            [
                new ApiResponseType
                {
                    ApiResponseFormats = [new ApiResponseFormat { MediaType = "application/json" }],
                    StatusCode = 200,
                    Type = typeof(Cat),
                },
            ]);

        var document = Subject([petApi, catApi]).GetSwagger("v1");

        // (a) The union response is an anyOf of $refs, not inlined object schemas.
        var unionSchema = document.Paths["/pet"].Operations[HttpMethod.Get].Responses["200"].Content["application/json"].Schema;

        Assert.NotNull(unionSchema.AnyOf);
        Assert.Equal(2, unionSchema.AnyOf.Count);

        var unionRefs = unionSchema.AnyOf
            .Select(s => Assert.IsType<OpenApiSchemaReference>(s))
            .Select(r => r.Reference.Id)
            .OrderBy(id => id)
            .ToArray();

        Assert.Equal([nameof(Cat), nameof(Dog)], unionRefs);

        // (b) The explicit Cat endpoint references the same component id used by the union case.
        var catSchema = document.Paths["/cat"].Operations[HttpMethod.Get].Responses["200"].Content["application/json"].Schema;
        var catReference = Assert.IsType<OpenApiSchemaReference>(catSchema);
        Assert.Equal(nameof(Cat), catReference.Reference.Id);

        // (c) The shared case type is registered exactly once (no Cat/Cat2 duplication).
        Assert.Equal(1, document.Components.Schemas.Keys.Count(id => id == nameof(Cat)));
        Assert.Equal(1, document.Components.Schemas.Keys.Count(id => id == nameof(Dog)));
        Assert.Equal([nameof(Cat), nameof(Dog)], document.Components.Schemas.Keys.OrderBy(id => id).ToArray());

        // Assert the same properties against the actually-serialized OpenAPI JSON.
        var json = ToJson(document);

        Assert.Contains("\"$ref\": \"#/components/schemas/Cat\"", json);
        Assert.Contains("\"$ref\": \"#/components/schemas/Dog\"", json);
        Assert.Contains("\"anyOf\"", json);

        // The union case schemas must not be inlined into the anyOf (no object body inside anyOf).
        var anyOfIndex = json.IndexOf("\"anyOf\"", StringComparison.Ordinal);
        var componentsIndex = json.IndexOf("\"components\"", StringComparison.Ordinal);
        var anyOfBlock = json[anyOfIndex..componentsIndex];
        Assert.DoesNotContain("\"properties\"", anyOfBlock);
        Assert.DoesNotContain("\"type\": \"object\"", anyOfBlock);

        // Cat is defined exactly once as a component schema.
        Assert.DoesNotContain("\"Cat2\"", json);
    }

    private static SwaggerGenerator Subject(IEnumerable<ApiDescription> apiDescriptions)
    {
        var schemaGeneratorOptions = new SchemaGeneratorOptions();

        // Production options come from DI with a configured resolver; a bare JsonSerializerOptions
        // has no TypeInfoResolver, which is required to inspect union metadata.
        var serializerOptions = new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

        return new SwaggerGenerator(
            new SwaggerGeneratorOptions
            {
                SwaggerDocs = new Dictionary<string, OpenApiInfo>
                {
                    ["v1"] = new OpenApiInfo { Version = "V1", Title = "Test API" },
                },
            },
            new FakeApiDescriptionGroupCollectionProvider(apiDescriptions),
            new SchemaGenerator(schemaGeneratorOptions, new JsonSerializerDataContractResolver(serializerOptions, schemaGeneratorOptions)),
            new FakeAuthenticationSchemeProvider([]));
    }

    private static string ToJson(OpenApiDocument document)
    {
        using var stringWriter = new StringWriter();
        var jsonWriter = new OpenApiJsonWriter(stringWriter);
        document.SerializeAs(OpenApiSpecVersion.OpenApi3_0, jsonWriter);
        return stringWriter.ToString();
    }
}
#endif
