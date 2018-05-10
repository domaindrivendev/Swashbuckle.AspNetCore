using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;
using Xunit;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class SwaggerResponseAttributeFilterTests
    {
        [Fact]
        public void Apply_SetsResponses_FromAttributes()
        {
            var operation = new Operation
            {
                OperationId = "foobar"
            };
            var filterContext = this.FilterContextFor(nameof(FakeActions.AnnotatedWithSwaggerResponseAttributes));

            this.Subject().Apply(operation, filterContext);

            var responses = operation.Responses;
            Assert.Equal(new[] { "204", "400" }, responses.Keys.ToArray());
            var response1 = responses["204"];
            Assert.Equal("No content is returned.", response1.Description);
            Assert.Null(response1.Schema);
            var response2 = responses["400"];
            Assert.Equal("This returns a dictionary.", response2.Description);
            Assert.NotNull(response2.Schema);
        }

        private OperationFilterContext FilterContextFor(
            string actionFixtureName,
            string controllerFixtureName = "NotAnnotated")
        {
            var fakeProvider = new FakeApiDescriptionGroupCollectionProvider();
            var apiDescription = fakeProvider
                .Add("GET", "collection", actionFixtureName, controllerFixtureName)
                .ApiDescriptionGroups.Items.First()
                .Items.First();

            return new OperationFilterContext(
                apiDescription,
                new SchemaRegistry(new JsonSerializerSettings()));
        }

        private SwaggerResponseAttributeFilter Subject()
        {
            return new SwaggerResponseAttributeFilter();
        }
    }
}
