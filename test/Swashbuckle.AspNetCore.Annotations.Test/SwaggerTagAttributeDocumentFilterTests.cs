using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations.Test.Fixtures;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;
using System.Linq;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class SwaggerTagAttributeDocumentFilterTests
    {
        [Fact]
        public void Apply_SetsTagWithDescription_UsingAttributeDescription()
        {
            var document = new SwaggerDocument();
            var filterContext = this.FilterContextFor<TestController>();

            Subject().Apply(document, filterContext);

            Assert.Single(document.Tags);
            Assert.Contains(document.Tags, tag => tag.Name == "TestTag" && tag.Description == "TestDescription");
        }

        [Fact]
        public void Apply_MultipleTags_AddsMultipleDocumentTags()
        {
            var document = new SwaggerDocument();
            var filterContext = this.FilterContextFor<TaggedController>();

            Subject().Apply(document, filterContext);

            Assert.Equal(3, document.Tags.Count);

            // This would be so much more pretty with FluentValidation or some such (Collection Equivalence checks)
            // Alternatively if Tag could implement IEquatable or there was a comparer.
            Assert.Contains(document.Tags, tag => tag.Name == "Tag1" && tag.Description == "Description1");
            Assert.Contains(document.Tags, tag => tag.Name == "Tag2" && tag.Description == "Description2");
            Assert.Contains(document.Tags, tag => tag.Name == "Tag42" && tag.Description == "Description42");
        }

        private DocumentFilterContext FilterContextFor<TController>()
        {
            var filterContext = new DocumentFilterContext(
                null,
                new[]
                {
                    new ApiDescription
                    {
                        ActionDescriptor = new ControllerActionDescriptor
                        {
                            ControllerTypeInfo = typeof(TController).GetTypeInfo(),
                            ControllerName = typeof(TController).Name
                        }
                    },
                },
                null);
            return filterContext;
        }

        private SwaggerTagAttributeDocumentFilter Subject()
        {
            return new SwaggerTagAttributeDocumentFilter();
        }
    }
}
