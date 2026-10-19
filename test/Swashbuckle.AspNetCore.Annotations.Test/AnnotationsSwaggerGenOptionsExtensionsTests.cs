using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
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
    [InlineData(typeof(FallbackShape), typeof(FallbackShape))]
    [InlineData(typeof(FallbackToNearestAncestorShape), typeof(FallbackToNearestAncestorShape))]
    [InlineData(typeof(AbstractFallbackShape), null)]
    [InlineData(typeof(FailSerializationShape), null)]
    public void EnableAnnotations_DiscriminatorDefaultMappingSelector_ReturnsExpectedType(Type baseType, Type expected)
    {
        var options = new SwaggerGenOptions();

        options.EnableAnnotations(enableAnnotationsForInheritance: true, enableAnnotationsForPolymorphism: true);

        var actual = options.SchemaGeneratorOptions.DiscriminatorDefaultMappingSelector(baseType);

        Assert.Equal(expected, actual);
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

    [JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
    [JsonDerivedType(typeof(FallbackCircle), "circle")]
    public class FallbackShape;

    public class FallbackCircle : FallbackShape;

    [JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
    [JsonDerivedType(typeof(NearestAncestorCircle), "circle")]
    public class FallbackToNearestAncestorShape;

    public class NearestAncestorCircle : FallbackToNearestAncestorShape;

    [JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
    [JsonDerivedType(typeof(AbstractFallbackCircle), "circle")]
    public abstract class AbstractFallbackShape;

    public class AbstractFallbackCircle : AbstractFallbackShape;

    [JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
    [JsonDerivedType(typeof(FailSerializationCircle), "circle")]
    public class FailSerializationShape;

    public class FailSerializationCircle : FailSerializationShape;
}
