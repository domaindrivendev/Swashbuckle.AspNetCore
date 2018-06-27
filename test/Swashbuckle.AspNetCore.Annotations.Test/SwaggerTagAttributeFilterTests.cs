using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Xunit;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class SwaggerTagAttributeFilterTests
    {
        [Fact]
        public void Apply_CreatesMetadataForControllerNameTag_FromSwaggerTagAttribute()
        {
            var document = new SwaggerDocument();
            var filterContext = FilterContextFor<TestController>();

            Subject().Apply(document, filterContext);

            var tag = document.Tags.Single(t => t.Name == "TestController");
            Assert.Equal("description for TestController", tag.Description);
            Assert.Equal("http://tempuri.org", tag.ExternalDocs.Url);
        }

        private DocumentFilterContext FilterContextFor<TController>()
        {
            return new DocumentFilterContext(
                null,
                new[]
                {
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(TController).GetTypeInfo(),
                            ControllerName = typeof(TController).Name
                        }
                    },
                },
                null);
        }

        private SwaggerTagAttributeFilter Subject()
        {
            return new SwaggerTagAttributeFilter();
        }
    }
}
