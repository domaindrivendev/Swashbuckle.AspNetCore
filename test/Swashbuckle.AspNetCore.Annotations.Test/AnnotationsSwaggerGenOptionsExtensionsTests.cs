using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Swashbuckle.AspNetCore.Annotations.Test;

public class AnnotationsSwaggerGenOptionsExtensionsTests
{
    [Fact]
    public void EnableAnnotations_ForPolymorphism_ConfiguresDiscriminatorSelectors()
    {
        var options = new SwaggerGenOptions();

        options.EnableAnnotations(enableAnnotationsForInheritance: false, enableAnnotationsForPolymorphism: true);

        var generatorOptions = options.SchemaGeneratorOptions;
        Assert.True(generatorOptions.UseOneOfForPolymorphism);
        Assert.Equal("kind", generatorOptions.DiscriminatorNameSelector(typeof(ShapeWithDefaultMapping)));
        Assert.Equal("circle", generatorOptions.DiscriminatorValueSelector(typeof(CircleShape)));
        Assert.Equal(typeof(CircleShape), generatorOptions.DiscriminatorDefaultMappingSelector(typeof(ShapeWithDefaultMapping)));
    }

    [Theory]
    [InlineData(typeof(ShapeWithDefaultMapping), typeof(CircleShape))]
    [InlineData(typeof(ShapeWithoutDefaultMapping), null)]
    [InlineData(typeof(IgnoreUnrecognizedShape), typeof(IgnoreUnrecognizedShape))]
    [InlineData(typeof(AbstractIgnoreUnrecognizedShape), null)]
    [InlineData(typeof(FailUnrecognizedShape), null)]
    [InlineData(typeof(FallbackShape), null)]
    [InlineData(typeof(FallbackToNearestAncestorShape), null)]
    public void EnableAnnotations_DiscriminatorDefaultMappingSelector_ReturnsExpectedType(Type baseType, Type expected)
    {
        var options = new SwaggerGenOptions();

        options.EnableAnnotations(enableAnnotationsForInheritance: true, enableAnnotationsForPolymorphism: true);

        var actual = options.SchemaGeneratorOptions.DiscriminatorDefaultMappingSelector(baseType);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void EnableAnnotations_IgnoreUnrecognizedTypeDiscriminators_GeneratesDefaultMappingThatDoesNotRequireDiscriminator()
    {
        var serializerOptions = new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

        // Payloads with a missing or unrecognized discriminator are deserialized as the base type
        Assert.IsType<IgnoreUnrecognizedShape>(JsonSerializer.Deserialize<IgnoreUnrecognizedShape>("{}", serializerOptions));
        Assert.IsType<IgnoreUnrecognizedShape>(JsonSerializer.Deserialize<IgnoreUnrecognizedShape>("""{"$type":"unknown"}""", serializerOptions));

        var schema = GenerateSchema(typeof(IgnoreUnrecognizedShape), serializerOptions);

        Assert.NotNull(schema.Discriminator);
        Assert.Equal("$type", schema.Discriminator.PropertyName);
        Assert.NotNull(schema.Discriminator.DefaultMapping);
        Assert.Equal(nameof(IgnoreUnrecognizedShape), schema.Discriminator.DefaultMapping.Reference.Id);
        Assert.Contains("$type", schema.Properties.Keys);
        Assert.DoesNotContain("$type", schema.Required);
    }

    [Theory]
    [InlineData(typeof(FailUnrecognizedShape))]
    [InlineData(typeof(FallbackShape))]
    public void EnableAnnotations_UnrecognizedTypeDiscriminatorsNotIgnored_GeneratesRequiredDiscriminatorWithoutDefaultMapping(Type baseType)
    {
        var serializerOptions = new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

        // Payloads with an unrecognized discriminator fail to deserialize
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("""{"$type":"unknown"}""", baseType, serializerOptions));

        var schema = GenerateSchema(baseType, serializerOptions);

        Assert.NotNull(schema.Discriminator);
        Assert.Equal("$type", schema.Discriminator.PropertyName);
        Assert.Null(schema.Discriminator.DefaultMapping);
        Assert.Contains("$type", schema.Required);
    }

    private static OpenApiSchema GenerateSchema(Type type, JsonSerializerOptions serializerOptions)
    {
        var options = new SwaggerGenOptions();
        options.EnableAnnotations(enableAnnotationsForInheritance: false, enableAnnotationsForPolymorphism: true);

        var generator = new SchemaGenerator(
            options.SchemaGeneratorOptions,
            new JsonSerializerDataContractResolver(serializerOptions, options.SchemaGeneratorOptions));

        var repository = new SchemaRepository();
        generator.GenerateSchema(type, repository);

        return Assert.IsAssignableFrom<OpenApiSchema>(repository.Schemas[type.Name]);
    }

    [SwaggerDiscriminator("kind", DefaultMapping = typeof(CircleShape))]
    [SwaggerSubType(typeof(CircleShape), DiscriminatorValue = "circle")]
    [SwaggerSubType(typeof(SquareShape), DiscriminatorValue = "square")]
    public abstract class ShapeWithDefaultMapping;

    [SwaggerDiscriminator("kind")]
    [SwaggerSubType(typeof(CircleShape), DiscriminatorValue = "circle")]
    public abstract class ShapeWithoutDefaultMapping;

    public class CircleShape : ShapeWithDefaultMapping;

    public class SquareShape : ShapeWithDefaultMapping;

    [JsonPolymorphic(IgnoreUnrecognizedTypeDiscriminators = true)]
    [JsonDerivedType(typeof(IgnoreUnrecognizedCircle), "circle")]
    public class IgnoreUnrecognizedShape;

    public class IgnoreUnrecognizedCircle : IgnoreUnrecognizedShape;

    [JsonPolymorphic(IgnoreUnrecognizedTypeDiscriminators = true)]
    [JsonDerivedType(typeof(AbstractIgnoreUnrecognizedCircle), "circle")]
    public abstract class AbstractIgnoreUnrecognizedShape;

    public class AbstractIgnoreUnrecognizedCircle : AbstractIgnoreUnrecognizedShape;

    [JsonPolymorphic(IgnoreUnrecognizedTypeDiscriminators = false)]
    [JsonDerivedType(typeof(FailUnrecognizedCircle), "circle")]
    public class FailUnrecognizedShape;

    public class FailUnrecognizedCircle : FailUnrecognizedShape;

    // UnknownDerivedTypeHandling only affects serialization, so unrecognized discriminators still fail deserialization
    [JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
    [JsonDerivedType(typeof(FallbackCircle), "circle")]
    public class FallbackShape;

    public class FallbackCircle : FallbackShape;

    [JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
    [JsonDerivedType(typeof(NearestAncestorCircle), "circle")]
    public class FallbackToNearestAncestorShape;

    public class NearestAncestorCircle : FallbackToNearestAncestorShape;
}
