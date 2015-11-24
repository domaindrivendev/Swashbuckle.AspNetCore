using System.Linq;
using Xunit;
using Swashbuckle.Swagger.Fixtures.ApiDescriptions;
using Swashbuckle.Swagger.Fixtures.Extensions;

namespace Swashbuckle.Swagger.Annotations
{
    public class ApplySwaggerOperationFilterAttributesTests
    {
        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfControllerAnnotatedWithFilterAttribute()
        {
            var operation = new Operation
            {
                OperationId = "foobar" 
            };
            var filterContext = FilterContextFor(
                nameof(ActionFixtures.ReturnsActionResult),
                nameof(ControllerFixtures.AnnotatedWithSwaggerOperationFilter)
            );

            Subject().Apply(operation, filterContext);

            Assert.NotEmpty(operation.Extensions);
        }

        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfActionAnnotatedWithFilterAttribute()
        {
            var operation = new Operation
            {
                OperationId = "foobar" 
            };
            var filterContext = FilterContextFor(nameof(ActionFixtures.AnnotatedWithSwaggerOperationFilter));

            Subject().Apply(operation, filterContext);

            Assert.NotEmpty(operation.Extensions);
        }

        private OperationFilterContext FilterContextFor(
            string actionFixtureName,
            string controllerFixtureName = "NotAnnotated"
        )
        {
            var fakeProvider = new FakeApiDescriptionGroupCollectionProvider();
            var apiDescription = fakeProvider
                .Add("GET", "collection", actionFixtureName, controllerFixtureName)
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