using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;
using Swashbuckle.SwaggerGen.Generator;
using Swashbuckle.SwaggerGen.TestFixtures;

namespace Swashbuckle.SwaggerGen.Annotations
{
    public class SwaggerAttributesOperationFilterTests
    {
        [Fact]
        public void Apply_AssignsProperties_FromActionAttribute()
        {
            var operation = new Operation
            {
                OperationId = "foobar" 
            };
            var filterContext = FilterContextFor(nameof(FakeActions.AnnotatedWithSwaggerOperation));

            Subject().Apply(operation, filterContext);

            Assert.Equal("CustomOperationId", operation.OperationId);
            Assert.Equal(new[] { "customTag" }, operation.Tags.ToArray());
            Assert.Equal(new[] { "customScheme" }, operation.Schemes.ToArray());
        }

        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfControllerAnnotatedWithFilterAttribute()
        {
            var operation = new Operation
            {
                OperationId = "foobar" 
            };
            var filterContext = FilterContextFor(
                nameof(FakeActions.ReturnsActionResult),
                nameof(FakeControllers.AnnotatedWithSwaggerOperationFilter)
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
            var filterContext = FilterContextFor(nameof(FakeActions.AnnotatedWithSwaggerOperationFilter));

            Subject().Apply(operation, filterContext);

            Assert.NotEmpty(operation.Extensions);
        }

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
                nameof(FakeActions.ReturnsActionResult),
                nameof(FakeControllers.AnnotatedWithSwaggerResponseRemoveDefaults)
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
            var filterContext = FilterContextFor(nameof(FakeActions.AnnotatedWithSwaggerResponseRemoveDefaults));

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
                nameof(FakeActions.ReturnsActionResult),
                nameof(FakeControllers.AnnotatedWithSwaggerResponses)
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
            var filterContext = FilterContextFor(nameof(FakeActions.AnnotatedWithSwaggerResponses));

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
                nameof(FakeActions.AnnotatedWithSwaggerResponses),
                nameof(FakeControllers.AnnotatedWithSwaggerResponses)
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
                new SchemaRegistry(new JsonSerializerSettings()));
        }

        private SwaggerAttributesOperationFilter Subject()
        {
            return new SwaggerAttributesOperationFilter();
        }
    }
}