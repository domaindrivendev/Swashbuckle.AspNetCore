using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Swashbuckle.AspNetCore.Annotations.Test;

public class AnnotationsSchemaFilterTests
{
    [Theory]
    [InlineData(typeof(SwaggerAnnotatedType))]
    [InlineData(typeof(SwaggerAnnotatedStruct))]
    public void Apply_EnrichesSchemaMetadata_IfTypeDecoratedWithSwaggerSchemaAttribute(Type type)
    {
        var schema = new OpenApiSchema();
        var context = new SchemaFilterContext(type: type, schemaGenerator: null, schemaRepository: null);

        Subject().Apply(schema, context);

        Assert.Equal($"Description for {type.Name}", schema.Description);
        Assert.Equal(["StringWithSwaggerSchemaAttribute"], schema.Required);
        Assert.Equal($"Title for {type.Name}", schema.Title);
    }

    [Fact]
    public void Apply_EnrichesSchemaMetadata_IfParameterDecoratedWithSwaggerSchemaAttribute()
    {
        var schema = new OpenApiSchema();
        var parameterInfo = typeof(FakeControllerWithSwaggerAnnotations)
            .GetMethod(nameof(FakeControllerWithSwaggerAnnotations.ActionWithSwaggerSchemaAttribute))
            .GetParameters()[0];
        var context = new SchemaFilterContext(
            type: parameterInfo.ParameterType,
            schemaGenerator: null,
            schemaRepository: null,
            parameterInfo: parameterInfo);

        Subject().Apply(schema, context);

        Assert.Equal($"Description for param", schema.Description);
        Assert.Equal("date", schema.Format);
    }

    [Theory]
    [InlineData(typeof(SwaggerAnnotatedType), nameof(SwaggerAnnotatedType.StringWithSwaggerSchemaAttribute), true, true, false)]
    [InlineData(typeof(SwaggerAnnotatedStruct), nameof(SwaggerAnnotatedStruct.StringWithSwaggerSchemaAttribute), true, true, false)]
    public void Apply_EnrichesSchemaMetadata_IfPropertyDecoratedWithSwaggerSchemaAttribute(
        Type declaringType,
        string propertyName,
        bool expectedReadOnly,
        bool expectedWriteOnly,
        bool expectedNullable)
    {
        var schema = new OpenApiSchema { Type = JsonSchemaType.Null };
        var propertyInfo = declaringType
            .GetProperty(propertyName);
        var context = new SchemaFilterContext(
            type: propertyInfo.PropertyType,
            schemaGenerator: null,
            schemaRepository: null,
            memberInfo: propertyInfo);

        Subject().Apply(schema, context);

        Assert.Equal($"Description for {propertyName}", schema.Description);
        Assert.Equal("date", schema.Format);
        Assert.Equal(expectedReadOnly, schema.ReadOnly);
        Assert.Equal(expectedWriteOnly, schema.WriteOnly);
        Assert.Equal(expectedNullable, schema.Type.Value.HasFlag(JsonSchemaType.Null));
    }

    [Fact]
    public void Apply_DoesNotModifyFlags_IfNotSpecifiedWithSwaggerSchemaAttribute()
    {
        var schema = new OpenApiSchema { ReadOnly = true, WriteOnly = true, Type = JsonSchemaType.Null };
        var propertyInfo = typeof(SwaggerAnnotatedType)
            .GetProperty(nameof(SwaggerAnnotatedType.StringWithSwaggerSchemaAttributeDescriptionOnly));
        var context = new SchemaFilterContext(
            type: propertyInfo.PropertyType,
            schemaGenerator: null,
            schemaRepository: null,
            memberInfo: propertyInfo);

        Subject().Apply(schema, context);

        Assert.True(schema.ReadOnly);
        Assert.True(schema.WriteOnly);
        Assert.True(schema.Type.Value.HasFlag(JsonSchemaType.Null));
    }

    [Fact]
    public void Apply_AddsNullSchema_IfNullableSchemaHasAnyOf()
    {
        var schema = new OpenApiSchema
        {
            AnyOf =
            [
                new OpenApiSchema { Type = JsonSchemaTypes.String }
            ]
        };

        Subject().Apply(schema, ContextForProperty(nameof(TypeWithNullableComposedSchemas.AnyOfProperty)));

        Assert.Null(schema.Type);
        Assert.NotNull(schema.AnyOf);
        Assert.Equal(2, schema.AnyOf.Count);
        Assert.Contains(schema.AnyOf, s => s.Type == JsonSchemaType.Null);
    }

    [Fact]
    public void Apply_AddsNullSchema_IfNullableSchemaHasOneOf()
    {
        var schema = new OpenApiSchema
        {
            OneOf =
            [
                new OpenApiSchema { Type = JsonSchemaTypes.String }
            ]
        };

        Subject().Apply(schema, ContextForProperty(nameof(TypeWithNullableComposedSchemas.OneOfProperty)));

        Assert.Null(schema.Type);
        Assert.NotNull(schema.OneOf);
        Assert.Equal(2, schema.OneOf.Count);
        Assert.Contains(schema.OneOf, s => s.Type == JsonSchemaType.Null);
    }

    [Fact]
    public void Apply_DoesNotAddNullSchema_IfNullableSchemaAnyOfAlreadyAllowsNull()
    {
        var schema = new OpenApiSchema
        {
            AnyOf =
            [
                new OpenApiSchema { Type = JsonSchemaTypes.String },
                new OpenApiSchema { Type = JsonSchemaType.Null },
            ]
        };

        Subject().Apply(schema, ContextForProperty(nameof(TypeWithNullableComposedSchemas.AnyOfProperty)));

        Assert.NotNull(schema.AnyOf);
        Assert.Equal(2, schema.AnyOf.Count);
        Assert.Single(schema.AnyOf, s => s.Type == JsonSchemaType.Null);
    }

    [Fact]
    public void Apply_DoesNotAddNullSchema_IfNullableSchemaOneOfAlreadyAllowsNull()
    {
        var schema = new OpenApiSchema
        {
            OneOf =
            [
                new OpenApiSchema { Type = JsonSchemaTypes.String },
                new OpenApiSchema { Type = JsonSchemaType.Null },
            ]
        };

        Subject().Apply(schema, ContextForProperty(nameof(TypeWithNullableComposedSchemas.OneOfProperty)));

        Assert.NotNull(schema.OneOf);
        Assert.Equal(2, schema.OneOf.Count);
        Assert.Single(schema.OneOf, s => s.Type == JsonSchemaType.Null);
    }

    [Fact]
    public void Apply_PreservesAllOf_IfNullableSchemaHasAllOf()
    {
        var schema = new OpenApiSchema
        {
            AllOf =
            [
                new OpenApiSchema { Type = JsonSchemaTypes.String }
            ]
        };

        Subject().Apply(schema, ContextForProperty(nameof(TypeWithNullableComposedSchemas.AllOfProperty)));

        // The composition is left intact: wrapping it in an anyOf would stop client
        // generators resolving the $ref once serialized as OpenAPI 3.0.
        Assert.Null(schema.AnyOf);
        Assert.NotNull(schema.AllOf);
        Assert.Single(schema.AllOf);
        Assert.True(schema.Type.HasValue);
        Assert.True(schema.Type.Value.HasFlag(JsonSchemaType.Null));
    }

    [Theory]
    [InlineData(typeof(SwaggerAnnotatedType))]
    [InlineData(typeof(SwaggerAnnotatedStruct))]
    public void Apply_DelegatesToSpecifiedFilter_IfTypeDecoratedWithFilterAttribute(Type type)
    {
        var schema = new OpenApiSchema();
        var context = new SchemaFilterContext(type: type, schemaGenerator: null, schemaRepository: null);

        Subject().Apply(schema, context);

        Assert.NotEmpty(schema.Extensions);
    }

    private static SchemaFilterContext ContextForProperty(string propertyName)
    {
        var propertyInfo = typeof(TypeWithNullableComposedSchemas).GetProperty(propertyName);
        return new SchemaFilterContext(
            type: propertyInfo.PropertyType,
            schemaGenerator: null,
            schemaRepository: null,
            memberInfo: propertyInfo);
    }

    private class TypeWithNullableComposedSchemas
    {
        [SwaggerSchema(Nullable = true)]
        public object AnyOfProperty { get; set; }

        [SwaggerSchema(Nullable = true)]
        public object OneOfProperty { get; set; }

        [SwaggerSchema(Nullable = true)]
        public object AllOfProperty { get; set; }
    }

    private static AnnotationsSchemaFilter Subject()
    {
        // A service provider is required from .NET 8 onwards.
        // See https://learn.microsoft.com/dotnet/core/compatibility/extensions/8.0/activatorutilities-createinstance-null-provider.
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        return new AnnotationsSchemaFilter(serviceProvider);
    }
}
