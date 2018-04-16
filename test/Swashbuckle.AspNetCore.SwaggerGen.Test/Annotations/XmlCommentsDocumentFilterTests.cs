using System.Xml.XPath;
using System.Reflection;
using System.IO;
using Xunit;
using Swashbuckle.AspNetCore.Swagger;
using System;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsDocumentFilterTests
    {
        [Fact]
        public void Apply_SetsTagDescription_FromControllerSummaryTags()
        {
            var document = new SwaggerDocument();
            var filterContext = new DocumentFilterContext(
                null,
                new[]
                {
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(FakeControllers.AnnotatedWithXml).GetTypeInfo(),
                            ControllerName = nameof(FakeControllers.AnnotatedWithXml)
                        }
                    },
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(FakeControllers.AnnotatedWithXml).GetTypeInfo(),
                            ControllerName = nameof(FakeControllers.AnnotatedWithXml)
                        }
                    }
                },
                null);

            Subject().Apply(document, filterContext);

            Assert.Equal(1, document.Tags.Count);
            Assert.Equal("summary for AnnotatedWithXml", document.Tags[0].Description);
        }

        private XmlCommentsDocumentFilter Subject()
        {
            using (var xmlComments = File.OpenText($"{GetType().GetTypeInfo().Assembly.GetName().Name}.xml"))
            {
                return new XmlCommentsDocumentFilter(new XPathDocument(xmlComments));
            }
        }
    }
}