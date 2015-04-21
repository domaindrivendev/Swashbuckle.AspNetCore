using System.Linq;
using Xunit;
using Swashbuckle.Fixtures.ApiDescriptions;

namespace Swashbuckle.Swagger.Annotations
{
    public class ApplySwaggerOperationAttributesTests
    {
        [Fact]
        public void Apply_AssignsProperties_FromActionAttribute()
        {
            var operation = new Operation
            {
                operationId = "foobar" 
            };
            var filterContext = FilterContextFor(nameof(ActionFixtures.AnnotatedWithSwaggerOperation));

            Subject().Apply(operation, filterContext);

            Assert.Equal("CustomOperationId", operation.operationId);
            Assert.Equal(new[] { "customTag" }, operation.tags.ToArray());
            Assert.Equal(new[] { "customScheme" }, operation.schemes.ToArray());
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

        private ApplySwaggerOperationAttributes Subject()
        {
            return new ApplySwaggerOperationAttributes();
        }
    }
}