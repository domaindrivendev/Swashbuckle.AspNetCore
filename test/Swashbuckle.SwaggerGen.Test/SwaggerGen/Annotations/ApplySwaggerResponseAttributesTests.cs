using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;
using Swashbuckle.SwaggerGen.Fixtures.ApiDescriptions;

namespace Swashbuckle.SwaggerGen.Annotations
{
    public class ApplySwaggerResponseAttributesTests
    {
        [Fact]
        public void Apply_RemovesExistingResponses_IfControllerAnnotatedWithRemoveDefaultsAttribute()
        {
            var operation = new Operation
            {
                Responses = new Dictionary<string, Response>
                {
                    { "200", new Response() }
                }
            };
            var filterContext = FilterContextFor(
                nameof(ActionFixtures.ReturnsActionResult),
                nameof(ControllerFixtures.AnnotatedWithSwaggerResponseRemoveDefaults)
            );

            Subject().Apply(operation, filterContext);

            Assert.Empty(operation.Responses);
        }

        [Fact]
        public void Apply_RemovesExistingResponses_IfActionAnnotatedWithRemoveDefaultsAttribute()
        {
            var operation = new Operation
            {
                Responses = new Dictionary<string, Response>
                {
                    { "200", new Response() }
                }
            };
            var filterContext = FilterContextFor(nameof(ActionFixtures.AnnotatedWithSwaggerResponseRemoveDefaults));

            Subject().Apply(operation, filterContext);

            Assert.Empty(operation.Responses);
        }

        [Fact]
        public void Apply_AddsResponses_FromControllerAttributes()
        {
            var operation = new Operation
            {
                Responses = new Dictionary<string, Response>
                {
                    { "200", new Response() }
                }
            };
            var filterContext = FilterContextFor(
                nameof(ActionFixtures.ReturnsActionResult),
                nameof(ControllerFixtures.AnnotatedWithSwaggerResponses)
            );

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "200", "400" }, operation.Responses.Keys.ToArray());
            Assert.Equal("Controller defined 200", operation.Responses["200"].Description);
            Assert.Equal("Controller defined 400", operation.Responses["400"].Description);
        }

        [Fact]
        public void Apply_AddsResponses_FromActionAttributes()
        {
            var operation = new Operation
            {
                Responses = new Dictionary<string, Response>
                {
                    { "200", new Response() }
                }
            };
            var filterContext = FilterContextFor(nameof(ActionFixtures.AnnotatedWithSwaggerResponses));

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "200", "400" }, operation.Responses.Keys.ToArray());
            Assert.Equal("Action defined 200", operation.Responses["200"].Description);
            Assert.Equal("Action defined 400", operation.Responses["400"].Description);
        }

        [Fact]
        public void Apply_FavorsActionAttributes_IfControllerAndActionBothAnnotatedWithSameStatusCode()
        {
            var operation = new Operation
            {
                Responses = new Dictionary<string, Response>
                {
                    { "200", new Response() }
                }
            };
            var filterContext = FilterContextFor(
                nameof(ActionFixtures.AnnotatedWithSwaggerResponses),
                nameof(ControllerFixtures.AnnotatedWithSwaggerResponses)
            );

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "200", "400" }, operation.Responses.Keys.ToArray());
            Assert.Equal("Action defined 200", operation.Responses["200"].Description);
            Assert.Equal("Action defined 400", operation.Responses["400"].Description);
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
                new DefaultSchemaRegistry(new JsonSerializerSettings()));
        }

        private ApplySwaggerResponseAttributes Subject()
        {
            return new ApplySwaggerResponseAttributes();
        }
    }
}