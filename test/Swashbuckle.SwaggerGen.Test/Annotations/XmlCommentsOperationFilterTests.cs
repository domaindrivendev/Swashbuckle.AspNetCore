using System.Linq;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Reflection;
using Xunit;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;
using Swashbuckle.SwaggerGen.TestFixtures;

namespace Swashbuckle.SwaggerGen.Annotations
{
    public class XmlCommentsOperationFilterTests
    {
        [Theory]
        [InlineData(nameof(FakeActions.AnnotatedWithXml))]
        public void Apply_SetsSummaryAndDescriptionFromSummaryAndRemarksTags(
            string actionFixtureName)
        {
            var operation = new Operation
            {
                Responses = new Dictionary<string, Response>()
            };
            var filterContext = FilterContextFor(actionFixtureName);

            Subject().Apply(operation, filterContext);

            Assert.Equal(string.Format("summary for {0}", actionFixtureName), operation.Summary);
            Assert.Equal(string.Format("remarks for {0}", actionFixtureName), operation.Description);
        }

        [Fact]
        public void Apply_SetsParameterDescriptionsFromParamTags()
        {
            var operation = new Operation
            {
                Parameters = new List<IParameter>
                {
                    new NonBodyParameter { Name = "param1" },
                    new NonBodyParameter { Name = "param2" }
                },
                Responses = new Dictionary<string, Response>()
            };
            var filterContext = FilterContextFor(nameof(FakeActions.AnnotatedWithXml));

            Subject().Apply(operation, filterContext);

            Assert.Equal("description for param1", operation.Parameters.First().Description);
            Assert.Equal("description for param2", operation.Parameters.Last().Description);
        }

        [Fact]
        public void Apply_OverwritesResponseDescriptionFromResponseTag_IfResponsePresent()
        {
            var operation = new Operation
            {
                Responses = new Dictionary<string, Response>
                {
                    { "200", new Response { Description = "Success", Schema = new Schema { Ref = "#/definitions/foo" } } },
                    { "400", new Response { Description = "Client Error", Schema = new Schema { Ref = "#/definitions/bar" } } }
                }
            };
            var filterContext = FilterContextFor(nameof(FakeActions.AnnotatedWithXml));

            Subject().Apply(operation, filterContext);

            Assert.Equal("description for 200", operation.Responses["200"].Description);
            Assert.NotNull(operation.Responses["200"].Schema.Ref);
            Assert.Equal("description for 400", operation.Responses["400"].Description);
            Assert.NotNull(operation.Responses["400"].Schema.Ref);
        }

        [Fact]
        public void Apply_AddsResponseWithDescriptionFromResponseTag_IfResponseNotPresent()
        {
            var operation = new Operation
            {
                Responses = new Dictionary<string, Response>
                {
                    { "200", new Response { Description = "Success", Schema = new Schema { Ref = "#/definitions/foo" } } },
                }
            };
            var filterContext = FilterContextFor(nameof(FakeActions.AnnotatedWithXml));

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "200", "400" }, operation.Responses.Keys.ToArray());
            Assert.Equal("description for 400", operation.Responses["400"].Description);
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

        private XmlCommentsOperationFilter Subject()
        {
            var xmlComments = GetType().GetTypeInfo()
                .Assembly
                .GetManifestResourceStream("Swashbuckle.SwaggerGen.Test.TestFixtures.XmlComments.xml");

            return new XmlCommentsOperationFilter(new XPathDocument(xmlComments));
        }
    }
}