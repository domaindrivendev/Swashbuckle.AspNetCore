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
                            ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                            ControllerName = nameof(FakeControllerWithXmlComments),
                            MethodInfo = typeof(FakeControllerWithXmlComments).GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParamTags))!
                        }
                    },
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                            ControllerName = nameof(FakeControllerWithXmlComments),
                            MethodInfo = typeof(FakeControllerWithXmlComments).GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParamTags))!
                        }
                    }
                },
                null,
                null);

            Subject().Apply(document, filterContext);

            var tag = Assert.Single(document.Tags);
            Assert.Equal("FakeControllerWithXmlComments", tag.Name);
            Assert.Equal("Summary for FakeControllerWithXmlComments", tag.Description);
        }

        [Fact]
        public void Apply_SetsCustomTagNameAndDescription_FromControllerAttributesAndSummaryTags()
        {
            var document = new OpenApiDocument();
            var filterContext = new DocumentFilterContext(
                new[]
                {
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(FakeControllerWithCustomTag).GetTypeInfo(),
                            ControllerName = nameof(FakeControllerWithCustomTag),
                            MethodInfo = typeof(FakeControllerWithCustomTag).GetMethod(nameof(FakeControllerWithCustomTag.ActionAny))!
                        }
                    },
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(FakeControllerWithCustomTag).GetTypeInfo(),
                            ControllerName = nameof(FakeControllerWithCustomTag),
                            MethodInfo = typeof(FakeControllerWithCustomTag).GetMethod(nameof(FakeControllerWithCustomTag.ActionAnother))!
                        }
                    }
                },
                null,
                null);

            Subject().Apply(document, filterContext);

            Assert.Equal(1, document.Tags.Count);
            Assert.Equal("fake controller custom tag", document.Tags[0].Name);
            Assert.Equal("Summary for FakeControllerWithCustomTag", document.Tags[0].Description);
        }

        [Fact]
        public void Apply_SetsTagNameWithNoDescription_ForControllerWithoutSummaryTags()
        {
            var document = new OpenApiDocument();
            var filterContext = new DocumentFilterContext(
                new[]
                {
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(FakeController).GetTypeInfo(),
                            ControllerName = nameof(FakeController),
                            MethodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ActionWithParameter))!
                        }
                    },
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(FakeControllerWithCustomTag).GetTypeInfo(),
                            ControllerName = nameof(FakeControllerWithCustomTag),
                            MethodInfo = typeof(FakeControllerWithCustomTag).GetMethod(nameof(FakeControllerWithCustomTag.ActionAny))!
                        }
                    }
                },
                null,
                null);

            Subject().Apply(document, filterContext);

            Assert.Equal(2, document.Tags.Count);
            Assert.Collection(document.Tags,
                tag1 =>
                {
                    Assert.Equal("fake controller custom tag", tag1.Name);
                    Assert.Equal("Summary for FakeControllerWithCustomTag", tag1.Description);
                },
                tag2 =>
                {
                    Assert.Equal("FakeController", tag2.Name);
                    Assert.Null(tag2.Description);
                });
        }

        private static XmlCommentsDocumentFilter Subject()
        {
            using (var xmlComments = File.OpenText($"{typeof(FakeControllerWithXmlComments).Assembly.GetName().Name}.xml"))
            {
                return new XmlCommentsDocumentFilter(new XPathDocument(xmlComments));
            }
        }
    }
}
