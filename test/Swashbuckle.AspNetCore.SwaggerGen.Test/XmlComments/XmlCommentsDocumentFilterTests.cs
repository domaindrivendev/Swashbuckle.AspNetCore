using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using System.Xml.XPath;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsDocumentFilterTests
    {
        [Theory]
        [InlineData(typeof(FakeControllerWithXmlComments))]
        [InlineData(typeof(FakeControllerWithInheritDoc))]
        public void Apply_SetsTagDescription_FromControllerSummaryTags(Type fakeController)
        {
            var document = new OpenApiDocument();
            var filterContext = new DocumentFilterContext(
                new[]
                {
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = fakeController.GetTypeInfo(),
                            ControllerName = nameof(fakeController)
                        }
                    },
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = fakeController.GetTypeInfo(),
                            ControllerName = nameof(fakeController)
                        }
                    }
                },
                null,
                null);

            Subject(fakeController).Apply(document, filterContext);

            Assert.Equal(1, document.Tags.Count);
            Assert.Equal("Summary for FakeControllerWithXmlComments", document.Tags[0].Description);
        }

        private XmlCommentsDocumentFilter Subject(Type fakeController)
        {
            using var xmlComments = File.OpenText($"{fakeController.Assembly.GetName().Name}.xml");
            return new XmlCommentsDocumentFilter(new XPathDocument(xmlComments));
        }
    }
}