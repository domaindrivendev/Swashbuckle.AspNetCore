using System.Linq;
using System.Collections.Generic;
using System.Xml.XPath;
using Xunit;
using Swashbuckle.SwaggerGen.Generator;
using Swashbuckle.SwaggerGen.TestFixtures;

namespace Swashbuckle.SwaggerGen.Annotations
{
    public class XmlCommentsOperationFilterTests
    {
        [Theory]
        [InlineData(nameof(FakeActions.AnnotatedWithXml))]
        public void Apply_SetsSummaryAndDescription_FromSummaryAndRemarksTags(
            string actionFixtureName)
        {
            var operation = new Operation();
            var filterContext = FilterContextFor(actionFixtureName);

            Subject().Apply(operation, filterContext);

            Assert.Equal(string.Format("summary for {0}", actionFixtureName), operation.Summary);
            Assert.Equal(string.Format("remarks for {0}", actionFixtureName), operation.Description);
        }

        [Fact]
        public void Apply_SetsParameterDescriptions_FromParamTags()
        {
            var operation = new Operation
            {
                Parameters = new List<IParameter>
                {
                    new NonBodyParameter { Name = "param1" },
                    new NonBodyParameter { Name = "param2" }
                }
            };
            var filterContext = FilterContextFor(nameof(FakeActions.AnnotatedWithXml));

            Subject().Apply(operation, filterContext);

            Assert.Equal("description for param1", operation.Parameters.First().Description);
            Assert.Equal("description for param2", operation.Parameters.Last().Description);
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
            return new XmlCommentsOperationFilter(new XPathDocument("TestFixtures/XmlComments.xml"));
        }
    }
}