using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;
using Swashbuckle.Fixtures;
using Swashbuckle.Fixtures.ApiDescriptions;

namespace Swashbuckle.Swagger.Annotations
{
    public class ApplySwaggerResponseAttributesTests
    {
        [Fact]
        public void Apply_RemovesExistingResponses_IfControllerDecoratedWithRemoveDefaultsAttribute()
        {
            var operation = new Operation
            {
                Responses = new Dictionary<string, Response>
                {
                    { "200", new Response() }
                }
            };
            var filterContext = FilterContextFor(nameof(ActionFixtures.ReturnsActionResult));
            filterContext.ApiDescription.ActionDescriptor.Properties.Add("ControllerAttributes",
                new[] { new SwaggerResponseRemoveDefaultsAttribute() });

            Subject().Apply(operation, filterContext);

            Assert.Empty(operation.Responses);
        }

        [Fact]
        public void Apply_RemovesExistingResponses_IfActionDecoratedWithRemoveDefaultsAttribute()
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
            var filterContext = FilterContextFor(nameof(ActionFixtures.ReturnsActionResult));
            filterContext.ApiDescription.ActionDescriptor.Properties.Add("ControllerAttributes",
                new[]
                {
                    new SwaggerResponseAttribute(500, "Internal Server Error", typeof(ComplexType)),
                    new SwaggerResponseAttribute(400, "Bad Request", typeof(ComplexType))
                });

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "200", "400", "500" }, operation.Responses.Keys.ToArray());
            Assert.Equal(new[] { "ComplexType" }, filterContext.SchemaRegistry.Definitions.Keys.ToArray());
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

            Assert.Equal(new[] { "200", "201", "202" }, operation.Responses.Keys.ToArray());
        }

        [Fact]
        public void Apply_FavorsActionAttributes_IfControllerAndActionBothDecoratedWithSameStatusCode()
        {
            var operation = new Operation
            {
                Responses = new Dictionary<string, Response>
                {
                    { "200", new Response() }
                }
            };
            var filterContext = FilterContextFor(nameof(ActionFixtures.AnnotatedWithSwaggerResponses));
            filterContext.ApiDescription.ActionDescriptor.Properties.Add("ControllerAttributes",
                new []
                {
                    new SwaggerResponseAttribute(201, "Created")
                });

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "200", "201", "202" }, operation.Responses.Keys.ToArray());
            Assert.Equal("ComplexType Created", operation.Responses["201"].Description);
        }


        private OperationFilterContext FilterContextFor(string actionFixtureName)
        {
            var fakeProvider = new FakeApiDescriptionGroupCollectionProvider();
            var apiDescription = fakeProvider
                .Add("GET", "collection", actionFixtureName)
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