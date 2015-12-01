using System.Linq;
using Xunit;
using Swashbuckle.SwaggerGen.Fixtures.ApiDescriptions;

namespace Swashbuckle.SwaggerGen.Annotations
{
    public class ApplySwaggerOperationAttributesTests
    {
        [Fact]
        public void Apply_AssignsProperties_FromActionAttribute()
        {
            var operation = new Operation
            {
                OperationId = "foobar" 
            };
            var filterContext = FilterContextFor(nameof(ActionFixtures.AnnotatedWithSwaggerOperation));

            Subject().Apply(operation, filterContext);

            Assert.Equal("CustomOperationId", operation.OperationId);
            Assert.Equal(new[] { "customTag" }, operation.Tags.ToArray());
            Assert.Equal(new[] { "customScheme" }, operation.Schemes.ToArray());
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