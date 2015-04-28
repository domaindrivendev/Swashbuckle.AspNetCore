using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Swashbuckle.Fixtures.ApiDescriptions;

namespace Swashbuckle.Swagger.XmlComments
{
    public class ApplyXmlActionCommentsTests
    {
        [Fact]
        public void Apply_SetsSummaryAndDescription_FromSummaryAndRemarksTags()
        {
            var operation = new Operation();
            var filterContext = FilterContextFor(nameof(ActionFixtures.AnnotatedWithSummaryAndRemarksXml));

            Subject().Apply(operation, filterContext);

            Assert.Equal("summary for AnnotatedWithSummaryAndRemarksXml", operation.summary);
            Assert.Equal("remarks for AnnotatedWithSummaryAndRemarksXml", operation.description);
        }

        [Fact]
        public void Apply_SetsParameterDescriptions_FromParamTags()
        {
            var operation = new Operation
            {
                parameters = new List<Parameter>
                {
                    new Parameter { name = "param1" },
                    new Parameter { name = "param2" }
                }
            };
            var filterContext = FilterContextFor(nameof(ActionFixtures.AnnotatedWithParamsXml));

            Subject().Apply(operation, filterContext);

            Assert.Equal("description for param1", operation.parameters.First().description);
            Assert.Equal("description for param2", operation.parameters.Last().description);
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

        private ApplyXmlActionComments Subject()
        {
            return new ApplyXmlActionComments("Fixtures/XmlComments.xml");
        }
    }
}