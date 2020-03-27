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
        [Fact]
        public void Apply_SetsTagDescription_FromControllerSummaryTags()
        {
            var document = new OpenApiDocument();
            var filterContext = new DocumentFilterContext(
                new[]
                {
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(ControllerWithXmlComments).GetTypeInfo(),
                            ControllerName = nameof(ControllerWithXmlComments)
                        }
                    },
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(ControllerWithXmlComments).GetTypeInfo(),
                            ControllerName = nameof(ControllerWithXmlComments)
                        }
                    }
                },
                null,
                null);

            Subject().Apply(document, filterContext);

            Assert.Equal(1, document.Tags.Count);
            Assert.Equal("Summary for ControllerWithXmlComments", document.Tags[0].Description);
        }

        private XmlCommentsDocumentFilter Subject()
        {
            using (var xmlComments = File.OpenText($"{typeof(ControllerWithXmlComments).Assembly.GetName().Name}.xml"))
            {
                return new XmlCommentsDocumentFilter(new XPathDocument(xmlComments));
            }
        }
    }
}