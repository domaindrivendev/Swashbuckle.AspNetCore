using System.Linq;
using Microsoft.OpenApi.Models;
using Xunit;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class AnnotationsDocumentFilterTests
    {
        [Fact]
        public void Apply_CreatesMetadataForControllerNameTag_FromSwaggerTagAttribute()
        {
            var document = new OpenApiDocument();
            var apiDescription = ApiDescriptionFactory.Create<FakeControllerWithSwaggerAnnotations>(c => nameof(c.ActionWithNoAttributes));
            var filterContext = new DocumentFilterContext(
                apiDescriptions: new[] { apiDescription },
                schemaGenerator: null,
                schemaRepository: null);

            Subject().Apply(document, filterContext);

            var tag = document.Tags.Single(t => t.Name == "FakeControllerWithSwaggerAnnotations");
            Assert.Equal("Description for FakeControllerWithSwaggerAnnotations", tag.Description);
            Assert.Equal("http://tempuri.org/", tag.ExternalDocs.Url.ToString());
        }

        private AnnotationsDocumentFilter Subject()
        {
            return new AnnotationsDocumentFilter();
        }
    }
}
