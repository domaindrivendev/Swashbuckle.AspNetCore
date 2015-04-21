using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
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
                responses = new Dictionary<string, Response>
                {
                    { "200", new Response() }
                }
            };
            var filterContext = FilterContextFor(nameof(ActionFixtures.ReturnsVoid));
            filterContext.ApiDescription.SetControllerAttributes(new[] { new SwaggerResponseRemoveDefaultsAttribute() });

            Subject().Apply(operation, filterContext);

            Assert.Empty(operation.responses);
        }

        [Fact]
        public void Apply_RemovesExistingResponses_IfActionDecoratedWithRemoveDefaultsAttribute()
        {
            var operation = new Operation
            {
                responses = new Dictionary<string, Response>
                {
                    { "200", new Response() }
                }
            };
            var filterContext = FilterContextFor(nameof(ActionFixtures.AnnotatedWithSwaggerResponseRemoveDefaults));
            filterContext.ApiDescription.SetControllerAttributes(new object[] { });

            Subject().Apply(operation, filterContext);

            Assert.Empty(operation.responses);
        }

        [Fact]
        public void Apply_AddsResponses_FromControllerAttributes()
        {
            var operation = new Operation
            {
                responses = new Dictionary<string, Response>
                {
                    { "200", new Response() }
                }
            };
            var filterContext = FilterContextFor(nameof(ActionFixtures.ReturnsVoid));
            filterContext.ApiDescription.SetControllerAttributes(new[]
            {
                new SwaggerResponseAttribute(500, "Internal Server Error", typeof(ComplexType)),
                new SwaggerResponseAttribute(400, "Bad Request", typeof(ComplexType))
            });

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "200", "400", "500" }, operation.responses.Keys.ToArray());
            Assert.Equal(new[] { "ComplexType" }, filterContext.SchemaRegistry.Definitions.Keys.ToArray());
        }

        [Fact]
        public void Apply_AddsResponses_FromActionAttributes()
        {
            var operation = new Operation
            {
                responses = new Dictionary<string, Response>
                {
                    { "200", new Response() }
                }
            };
            var filterContext = FilterContextFor(nameof(ActionFixtures.AnnotatedWithSwaggerResponses));
            filterContext.ApiDescription.SetControllerAttributes(new object[] { });

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "200", "201", "202" }, operation.responses.Keys.ToArray());
        }

        [Fact]
        public void Apply_FavorsActionAttributes_IfControllerAndActionBothDecoratedWithSameStatusCode()
        {
            var operation = new Operation
            {
                responses = new Dictionary<string, Response>
                {
                    { "200", new Response() }
                }
            };
            var filterContext = FilterContextFor(nameof(ActionFixtures.AnnotatedWithSwaggerResponses));
            filterContext.ApiDescription.SetControllerAttributes(new []
            {
                new SwaggerResponseAttribute(201, "Created")
            });

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "200", "201", "202" }, operation.responses.Keys.ToArray());
            Assert.Equal("ComplexType Created", operation.responses["201"].description);
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
                new SchemaGenerator(new DefaultContractResolver()));
        }

        private ApplySwaggerResponseAttributes Subject()
        {
            return new ApplySwaggerResponseAttributes();
        }
    }
}