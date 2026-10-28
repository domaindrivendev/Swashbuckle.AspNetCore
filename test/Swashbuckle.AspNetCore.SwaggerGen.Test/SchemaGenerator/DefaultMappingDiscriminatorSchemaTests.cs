using System.Text.Json;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class DefaultMappingDiscriminatorSchemaTests
{
    public static TheoryData<OpenApiSpecVersion> Versions =>
    [
        OpenApiSpecVersion.OpenApi2_0,
        OpenApiSpecVersion.OpenApi3_0,
        OpenApiSpecVersion.OpenApi3_1,
    ];

    [Theory]
    [MemberData(nameof(Versions))]
    public void Serialize_WithoutDiscriminator_DoesNotAddRequiredProperty(OpenApiSpecVersion version)
    {
        var subject = new DefaultMappingDiscriminatorSchema
        {
            Type = JsonSchemaTypes.Object,
            Required = new SortedSet<string>() { "name" },
        };

        var required = SerializeAndGetRequired(subject, version);

        Assert.Equal(["name"], required);
    }

    [Theory]
    [MemberData(nameof(Versions))]
    public void Serialize_WithoutDefaultMapping_DoesNotAddRequiredProperty(OpenApiSpecVersion version)
    {
        var subject = new DefaultMappingDiscriminatorSchema
        {
            Type = JsonSchemaTypes.Object,
            Discriminator = new OpenApiDiscriminator { PropertyName = "kind" },
        };

        var required = SerializeAndGetRequired(subject, version);

        Assert.Empty(required);
    }

    [Theory]
    [MemberData(nameof(Versions))]
    public void Serialize_WithoutDiscriminatorPropertyName_DoesNotAddRequiredProperty(OpenApiSpecVersion version)
    {
        var subject = new DefaultMappingDiscriminatorSchema
        {
            Type = JsonSchemaTypes.Object,
            Discriminator = new OpenApiDiscriminator { DefaultMapping = new OpenApiSchemaReference("Cat") },
        };

        var required = SerializeAndGetRequired(subject, version);

        Assert.Empty(required);
    }

    [Theory]
    [MemberData(nameof(Versions))]
    public void Serialize_WithDiscriminatorPropertyAlreadyRequired_DoesNotDuplicateRequiredProperty(OpenApiSpecVersion version)
    {
        var required = new SortedSet<string>() { "kind", "name" };
        var subject = new DefaultMappingDiscriminatorSchema
        {
            Type = JsonSchemaTypes.Object,
            Discriminator = new OpenApiDiscriminator
            {
                PropertyName = "kind",
                DefaultMapping = new OpenApiSchemaReference("Cat"),
            },
            Required = required,
        };

        var actual = SerializeAndGetRequired(subject, version);

        Assert.Equal(["kind", "name"], actual);
        Assert.Same(required, subject.Required);
    }

    [Theory]
    [MemberData(nameof(Versions))]
    public void Serialize_WithDefaultMappingAndNoRequiredProperties_RequiresDiscriminatorProperty(OpenApiSpecVersion version)
    {
        var subject = new DefaultMappingDiscriminatorSchema
        {
            Type = JsonSchemaTypes.Object,
            Discriminator = new OpenApiDiscriminator
            {
                PropertyName = "kind",
                DefaultMapping = new OpenApiSchemaReference("Cat"),
            },
            Required = null,
        };

        var required = SerializeAndGetRequired(subject, version);

        Assert.Equal(["kind"], required);
        Assert.Null(subject.Required);
    }

    [Theory]
    [MemberData(nameof(Versions))]
    public void Serialize_WithDefaultMappingAndOtherRequiredProperties_RequiresDiscriminatorProperty(OpenApiSpecVersion version)
    {
        var subject = new DefaultMappingDiscriminatorSchema
        {
            Type = JsonSchemaTypes.Object,
            Discriminator = new OpenApiDiscriminator
            {
                PropertyName = "kind",
                DefaultMapping = new OpenApiSchemaReference("Cat"),
            },
            Required = new SortedSet<string>() { "name" },
        };

        var required = SerializeAndGetRequired(subject, version);

        Assert.Equal(["kind", "name"], required);
        Assert.Equal(["name"], subject.Required);
    }

    private static string[] SerializeAndGetRequired(OpenApiSchema schema, OpenApiSpecVersion version)
    {
        using var writer = new StringWriter();
        var jsonWriter = new OpenApiJsonWriter(writer);

        switch (version)
        {
            case OpenApiSpecVersion.OpenApi2_0:
                schema.SerializeAsV2(jsonWriter);
                break;

            case OpenApiSpecVersion.OpenApi3_0:
                schema.SerializeAsV3(jsonWriter);
                break;

            default:
                schema.SerializeAsV31(jsonWriter);
                break;
        }

        using var json = JsonDocument.Parse(writer.ToString());

        return json.RootElement.TryGetProperty("required", out var value)
            ? [.. value.EnumerateArray().Select((p) => p.GetString())]
            : [];
    }
}
