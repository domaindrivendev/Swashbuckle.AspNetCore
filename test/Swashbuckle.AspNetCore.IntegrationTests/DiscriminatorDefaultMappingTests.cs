using System.Text.Json;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Swashbuckle.AspNetCore.IntegrationTests;

public class DiscriminatorDefaultMappingTests
{
    [Fact]
    public async Task PayloadWithoutDiscriminatorOnlyMatchesBaseTypeDefaultMapping()
    {
        // Arrange
        Assert.IsType<Shape>(JsonSerializer.Deserialize<Shape>("{}", SchemaValidator.SerializerOptions));
        Assert.IsType<Circle>(JsonSerializer.Deserialize<Shape>("""{"$type":"circle"}""", SchemaValidator.SerializerOptions));

        var validator = await SchemaValidator.CreateAsync<Shape>();

        // Act and Assert
        Assert.Equal(1, SchemaValidator.CountMatchingAlternatives(validator, "{}"));
        Assert.Empty(validator.Validate("{}"));
    }

    [Fact]
    public async Task PayloadWithoutDiscriminatorOnlyMatchesSubtypeDefaultMappingOfAbstractBaseType()
    {
        // Arrange
        var validator = await SchemaValidator.CreateAsync<Animal>();

        // Act and Assert
        Assert.Equal(1, SchemaValidator.CountMatchingAlternatives(validator, "{}"));
        Assert.Empty(validator.Validate("{}"));
    }

    [Fact]
    public async Task PayloadWithoutDiscriminatorOnlyMatchesSubtypeDefaultMappingOfConcreteBaseType()
    {
        // Arrange
        var validator = await SchemaValidator.CreateAsync<Vehicle>();

        // Act and Assert
        Assert.Equal(3, validator.OneOf.Count);
        Assert.Equal(1, SchemaValidator.CountMatchingAlternatives(validator, "{}"));
        Assert.Empty(validator.Validate("{}"));
    }

    [JsonPolymorphic(IgnoreUnrecognizedTypeDiscriminators = true)]
    [JsonDerivedType(typeof(Circle), "circle")]
    public class Shape;

    public class Circle : Shape;

    [SwaggerDiscriminator("kind", DefaultMapping = typeof(Cat))]
    [SwaggerSubType(typeof(Cat), DiscriminatorValue = "cat")]
    [SwaggerSubType(typeof(Dog), DiscriminatorValue = "dog")]
    public abstract class Animal;

    public class Cat : Animal;

    public class Dog : Animal;

    [SwaggerDiscriminator("kind", DefaultMapping = typeof(Car))]
    [SwaggerSubType(typeof(Car), DiscriminatorValue = "car")]
    [SwaggerSubType(typeof(Truck), DiscriminatorValue = "truck")]
    public class Vehicle;

    public class Car : Vehicle;

    public class Truck : Vehicle;
}
