using System.Globalization;
using System.Xml.XPath;
using System.Reflection;
using System.IO;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsDocumentFilterTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("en-US")]
        [InlineData("ru-RU")]
        public void Apply_SetsTagDescription_FromControllerSummaryTags(string cultureName)
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

            Subject(cultureName).Apply(document, filterContext);

            Assert.Equal(1, document.Tags.Count);
            switch (cultureName)
            {
                case "ru-RU":
                    Assert.Equal("Summary для FakeControllerWithXmlComments с русской локализацией", document.Tags[0].Description);
                    break;
                case "en-US":
                default:
                    Assert.Equal("Summary for FakeControllerWithXmlComments", document.Tags[0].Description);
                    break;
            }
        }

        private XmlCommentsDocumentFilter Subject(string cultureName = null)
        {
            using (var xmlComments = File.OpenText($"{typeof(FakeControllerWithXmlComments).Assembly.GetName().Name}.xml"))
            {
                var culture = cultureName == null ? null : new CultureInfo(cultureName);
                return new XmlCommentsDocumentFilter(new XPathDocument(xmlComments), culture);
            }
        }
    }
}