<<<<<<< HEAD
﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.XPath;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
=======
﻿using Microsoft.AspNetCore.Mvc.ApiExplorer;
>>>>>>> d7d252cf (=Move the XML node selection to a new static class that can search recursively using the `inheritdoc` as a reference. Point all XPath queries to this class. Simplify some of the (if) nesting in the XML Comment Filters. Add unit tests for the new InheritDoc updates. some existing tests were converted from `Fact` to `Theory` to accomplish this.)
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
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

            var tag = Assert.Single(document.Tags);
            Assert.Equal("Summary for FakeControllerWithXmlComments", tag.Description);
        }

<<<<<<< HEAD
        private static XmlCommentsDocumentFilter Subject()
=======
        private XmlCommentsDocumentFilter Subject(Type fakeController)
>>>>>>> d7d252cf (=Move the XML node selection to a new static class that can search recursively using the `inheritdoc` as a reference. Point all XPath queries to this class. Simplify some of the (if) nesting in the XML Comment Filters. Add unit tests for the new InheritDoc updates. some existing tests were converted from `Fact` to `Theory` to accomplish this.)
        {
            using var xmlComments = File.OpenText($"{fakeController.Assembly.GetName().Name}.xml");
            return new XmlCommentsDocumentFilter(new XPathDocument(xmlComments));
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
