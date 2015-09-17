using System.Linq;
using Xunit;
using Swashbuckle.Fixtures.ApiDescriptions;
using Swashbuckle.Fixtures.Extensions;

namespace Swashbuckle.Swagger.Annotations
{
    public class ApplySwaggerOperationFilterAttributesTests
    {
        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfControllerDecoratedWithFilterAttribute()
        {
            var operation = new Operation
            {
                OperationId = "foobar" 
            };
            var filterContext = FilterContextFor(nameof(ActionFixtures.ReturnsActionResult));
            filterContext.ApiDescription.ActionDescriptor.Properties.Add("ControllerAttributes",
                new[] { new SwaggerOperationFilterAttribute(typeof(VendorExtensionsOperationFilter)) });

            Subject().Apply(operation, filterContext);

            Assert.NotEmpty(operation.Extensions);
        }

        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfActionDecoratedWithFilterAttribute()
        {
            var operation = new Operation
            {
                OperationId = "foobar" 
            };
            var filterContext = FilterContextFor(nameof(ActionFixtures.AnnotatedWithSwaggerOperationFilter));

            Subject().Apply(operation, filterContext);

            Assert.NotEmpty(operation.Extensions);
        }

        private OperationFilterContext FilterContextFor(string actionFixtureName)
        {
            var fakeProvider = new FakeApiDescriptionGroupCollectionProvider();
            var apiDescription = fakeProvider
                .Add("GET", "collection", actionFixtureName)
                .ApiDescriptionGroups.Items.First()
                .Items.First();

            return new OperationFilterContext(apiDescription, null);
        }

        private ApplySwaggerOperationFilterAttributes Subject()
        {
            return new ApplySwaggerOperationFilterAttributes();
        }
    }
}