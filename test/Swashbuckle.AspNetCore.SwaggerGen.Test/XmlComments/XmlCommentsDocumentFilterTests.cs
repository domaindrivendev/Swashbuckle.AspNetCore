using System.Xml.XPath;
using System.Collections.Generic;
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
            var options = new SwaggerGeneratorOptions();
            var document = new OpenApiDocument();
            var filterContext = new DocumentFilterContext(
                new[]
                {
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                            ControllerName = nameof(FakeControllerWithXmlComments),
                            RouteValues = new Dictionary<string, string> { { "controller", nameof(FakeControllerWithXmlComments) } }
                        }
                    },
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                            ControllerName = nameof(FakeControllerWithXmlComments),
                            RouteValues = new Dictionary<string, string> { { "controller", nameof(FakeControllerWithXmlComments) } }
                        }
                    }
                },
                null,
                null);

            Subject(options).Apply(document, filterContext);

            var tag = Assert.Single(document.Tags);
            Assert.Equal("Summary for FakeControllerWithXmlComments", tag.Description);
        }

        [Fact]
        public void Uses_Proper_Tag_Name()
        {
            var options = new SwaggerGeneratorOptions();
            var document = new OpenApiDocument();
            var expectedTagName = "AliasControllerWithXmlComments";
            var filterContext = new DocumentFilterContext(
                new[]
                {
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                            ControllerName = nameof(FakeControllerWithXmlComments),
                            RouteValues = new Dictionary<string, string> { { "controller", expectedTagName } }
                        }
                    },
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                            ControllerName = nameof(FakeControllerWithXmlComments),
                            RouteValues = new Dictionary<string, string> { { "controller", expectedTagName } }
                        }
                    }
                },
                null,
                null);

            Subject(options).Apply(document, filterContext);

            var tag = Assert.Single(document.Tags);
            Assert.Equal(expectedTagName, tag.Name);
            Assert.Equal("Summary for FakeControllerWithXmlComments", tag.Description);
        }

        private static XmlCommentsDocumentFilter Subject(SwaggerGeneratorOptions options)
        {
            using (var xmlComments = File.OpenText($"{typeof(FakeControllerWithXmlComments).Assembly.GetName().Name}.xml"))
            {
                return new XmlCommentsDocumentFilter(options, new XPathDocument(xmlComments));
            }
        }
    }
}
