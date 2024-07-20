using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.XPath;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Xunit;

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

        [Fact]
        public void Ensure_IncludeXmlComments_Adds_Filter_To_Options()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IWebHostEnvironment, DummyHostEnvironment>();
            services.AddSwaggerGen(c =>
            {
                c.IncludeXmlComments(
                    typeof(FakeControllerWithXmlComments).Assembly,
                    includeControllerXmlComments: true);
            });

            using var provider = services.BuildServiceProvider();
            var options = provider.GetService<Microsoft.Extensions.Options.IOptions<SwaggerGeneratorOptions>>().Value;

            Assert.NotNull(options);
            Assert.Contains(options.DocumentFilters, x => x is XmlCommentsDocumentFilter);
        }

        private sealed class DummyHostEnvironment : IWebHostEnvironment
        {
            public string WebRootPath { get; set; }
            public IFileProvider WebRootFileProvider { get; set; }
            public string ApplicationName { get; set; }
            public IFileProvider ContentRootFileProvider { get; set; }
            public string ContentRootPath { get; set; }
            public string EnvironmentName { get; set; }
        }
    }
}
