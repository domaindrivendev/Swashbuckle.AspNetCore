using System.Linq;
using System.Collections.Generic;
using Xunit;
using Swashbuckle.SwaggerGen.Generator;
using Swashbuckle.SwaggerGen.TestFixtures.ApiDescriptions;

namespace Swashbuckle.SwaggerGen.Annotations
{
    public class XmlCommentsOperationFilterTests
    {
        [Fact]
        public void Apply_SetsSummaryAndDescription_FromSummaryAndRemarksTags()
        {
            var operation = new Operation();
            var filterContext = FilterContextFor(nameof(ActionFixtures.AnnotatedWithSummaryAndRemarksXml));

            Subject().Apply(operation, filterContext);

            Assert.Equal("summary for AnnotatedWithSummaryAndRemarksXml", operation.Summary);
            Assert.Equal("remarks for AnnotatedWithSummaryAndRemarksXml", operation.Description);
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
            var filterContext = FilterContextFor(nameof(ActionFixtures.AnnotatedWithParamsXml));

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
            return new XmlCommentsOperationFilter("TestFixtures/XmlComments.xml");
        }
    }
}