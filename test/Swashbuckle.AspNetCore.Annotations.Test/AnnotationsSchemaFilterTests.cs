using System;
using Microsoft.OpenApi.Models;
using Xunit;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
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
            Assert.Equal(new[] { "StringWithSwaggerSchemaAttribute" }, schema.Required);
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
        [InlineData(typeof(SwaggerAnnotatedType), nameof(SwaggerAnnotatedType.StringWithSwaggerSchemaAttribute), true, true)]
        [InlineData(typeof(SwaggerAnnotatedStruct), nameof(SwaggerAnnotatedStruct.StringWithSwaggerSchemaAttribute), true, true)]
        public void Apply_EnrichesSchemaMetadata_IfPropertyDecoratedWithSwaggerSchemaAttribute(
            Type declaringType,
            string propertyName,
            bool expectedReadOnly,
            bool expectedWriteOnly)
        {
            var schema = new OpenApiSchema();
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
        }

        [Fact]
        public void Apply_DoesNotModifyTheReadOnlyAndWriteOnlyFlags_IfNotSpecifiedWithSwaggerSchemaAttribute()
        {
            var schema = new OpenApiSchema { ReadOnly = true, WriteOnly = true };
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

        private AnnotationsSchemaFilter Subject()
        {
            return new AnnotationsSchemaFilter(null);
        }
    }
}