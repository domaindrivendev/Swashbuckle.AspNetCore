using System;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xunit;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class AnnotationsSchemaFilterTests
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public AnnotationsSchemaFilterTests()
        {
            _modelMetadataProvider = ModelMetadataHelper.GetDefaultModelMetadataProvider();
        }

        [Theory]
        [InlineData(typeof(SwaggerAnnotatedClass))]
        [InlineData(typeof(SwaggerAnnotatedStruct))]
        public void Apply_DelegatesToSpecifiedFilter_IfTypeDecoratedWithFilterAttribute(Type type)
        {
            var schema = new OpenApiSchema();
            var context = FilterContextFor(type);

            Subject().Apply(schema, context);

            Assert.NotEmpty(schema.Extensions);
        }

        private SchemaFilterContext FilterContextFor(Type type)
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(type);

            return new SchemaFilterContext(
                modelMetadata: modelMetadata,
                schemaRepository: null, // NA for test
                schemaGenerator: null, // NA for test
                jsonContract: null // NA for test
            );
        }

        private AnnotationsSchemaFilter Subject()
        {
            return new AnnotationsSchemaFilter(null);
        }
    }
}