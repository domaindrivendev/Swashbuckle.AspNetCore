using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Xunit;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class AnnotationsParameterFilterTests
    {
        [Fact]
        public void Apply_EnrichesParameterMetadata_IfParameterDecoratedWithSwaggerParameterAttribute()
        {
            var parameter = new OpenApiParameter { };
            var parameterInfo = typeof(FakeControllerWithSwaggerAnnotations)
                .GetMethod(nameof(FakeControllerWithSwaggerAnnotations.ActionWithSwaggerParameterAttribute))
                .GetParameters()[0];
            var filterContext = new ParameterFilterContext(
                apiParameterDescription: null,
                schemaGenerator: null,
                schemaRepository: null,
                parameterInfo: parameterInfo);

            Subject().Apply(parameter, filterContext);

            Assert.Equal("Description for param", parameter.Description);
            Assert.True(parameter.Required);
        }

        [Fact]
        public void Apply_EnrichesParameterMetadata_IfPropertyDecoratedWithSwaggerParameterAttribute()
        {
            var parameter = new OpenApiParameter();
            var propertyInfo = typeof(SwaggerAnnotatedType).GetProperty(nameof(SwaggerAnnotatedType.StringWithSwaggerParameterAttribute));
            var filterContext = new ParameterFilterContext(
                apiParameterDescription: new ApiParameterDescription(),
                schemaGenerator: null,
                schemaRepository: null,
                propertyInfo: propertyInfo);

            Subject().Apply(parameter, filterContext);

            Assert.Equal("Description for StringWithSwaggerParameterAttribute", parameter.Description);
            Assert.True(parameter.Required);
        }

        [Fact]
        public void Apply_DoesNotModifyTheRequiredFlag_IfNotSpecifiedWithSwaggerParameterAttribute()
        {
            var parameter = new OpenApiParameter { Required = true };
            var parameterInfo = typeof(FakeControllerWithSwaggerAnnotations)
                .GetMethod(nameof(FakeControllerWithSwaggerAnnotations.ActionWithSwaggerParameterAttributeDescriptionOnly))
                .GetParameters()[0];
            var filterContext = new ParameterFilterContext(
                apiParameterDescription: null,
                schemaGenerator: null,
                schemaRepository: null,
                parameterInfo: parameterInfo);

            Subject().Apply(parameter, filterContext);

            Assert.True(parameter.Required);
        }

        private AnnotationsParameterFilter Subject()
        {
            return new AnnotationsParameterFilter();
        }
    }
}
