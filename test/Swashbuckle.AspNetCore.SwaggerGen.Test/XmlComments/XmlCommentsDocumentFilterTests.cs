using System.Xml.XPath;
using System.Reflection;
using System.IO;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Xunit;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsDocumentFilterTests
    {
        [Theory]
        [InlineData("Summary for FakeControllerWithXmlComments (Remarks for FakeControllerWithXmlComments)", true)]
        [InlineData("Summary for FakeControllerWithXmlComments", false)]
        public void Apply_SetsTagDescription_FromControllerSummaryAndRemarksTags(
            string expectedDescription,
            bool includeRemarksFromXmlComments)
        {
            var document = new OpenApiDocument();
            var filterContext = new DocumentFilterContext(
                new[]
                {
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                            ControllerName = nameof(FakeControllerWithXmlComments)
                        }
                    },
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                            ControllerName = nameof(FakeControllerWithXmlComments)
                        }
                    }
                },
                null,
                null);

            Subject(includeRemarksFromXmlComments).Apply(document, filterContext);

            Assert.Equal(1, document.Tags.Count);
            Assert.Equal(expectedDescription, document.Tags[0].Description);
        }

        private XmlCommentsDocumentFilter Subject(bool includeRemarksFromXmlComments = false)
        {
            using (var xmlComments = File.OpenText($"{typeof(FakeControllerWithXmlComments).Assembly.GetName().Name}.xml"))
            {
                return new XmlCommentsDocumentFilter(new XPathDocument(xmlComments), includeRemarksFromXmlComments);
            }
        }
    }
}