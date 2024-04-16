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

            Subject().Apply(document, filterContext);

            var tag = Assert.Single(document.Tags);
            Assert.Equal("Summary for FakeControllerWithXmlComments", tag.Description);
        }

        private static XmlCommentsDocumentFilter Subject()
        {
            using (var xmlComments = File.OpenText($"{typeof(FakeControllerWithXmlComments).Assembly.GetName().Name}.xml"))
            {
                return new XmlCommentsDocumentFilter(new XPathDocument(xmlComments));
            }
        }

        [Fact]
        public void Uses_Proper_Tag_Name()
        {
            var expectedTagName = "AliasControllerWithXmlComments";
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
        }

        [Fact]
        public void Uses_Proper_Tag_Name_With_Custom_TagSelector()
        {
            var expectedTagName = "AliasControllerWithXmlComments";
            var options = new SwaggerGeneratorOptions { TagsSelector = apiDesc => new[] { expectedTagName } };
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
                        }
                    },
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                            ControllerName = nameof(FakeControllerWithXmlComments),
                        }
                    }
                },
                null,
                null);

            Subject(options).Apply(document, filterContext);

            var tag = Assert.Single(document.Tags);
            Assert.Equal(expectedTagName, tag.Name);
        }

        private static XmlCommentsDocumentFilter Subject(SwaggerGeneratorOptions options)
        {
            using (var xmlComments = File.OpenText($"{typeof(FakeControllerWithXmlComments).Assembly.GetName().Name}.xml"))
            {
                return new XmlCommentsDocumentFilter(new XPathDocument(xmlComments), options);
            }
        }
    }
}
