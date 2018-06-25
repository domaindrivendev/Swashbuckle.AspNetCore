using System.Linq;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Reflection;
using System.IO;
using Xunit;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsOperationFilterTests
    {
        [Fact]
        public void Apply_SetsSummaryAndDescription_FromSummaryAndRemarksTags()
        {
            var operation = new Operation
            {
                Responses = new Dictionary<string, Response>()
            };
            var filterContext = FilterContextFor(nameof(FakeController.AnnotatedWithXml));

            Subject().Apply(operation, filterContext);

            Assert.Equal("summary for AnnotatedWithXml", operation.Summary);
            Assert.Equal("remarks for AnnotatedWithXml", operation.Description);
        }

        [Fact]
        public void Apply_SetsParameterDescriptions_FromParamTags()
        {
            var operation = new Operation
            {
                Parameters = new List<IParameter>
                {
                    new NonBodyParameter { Name = "param1" }, 
                    new NonBodyParameter { Name = "param2" }, 
                    new NonBodyParameter { Name = "Param-3" } 
                },
                Responses = new Dictionary<string, Response>()
            };
            var filterContext = FilterContextFor(nameof(FakeController.AnnotatedWithXml));

            Subject().Apply(operation, filterContext);

            Assert.Equal("description for param1", operation.Parameters[0].Description);
            Assert.Equal("description for param2", operation.Parameters[1].Description);
            Assert.Equal("description for param3", operation.Parameters[2].Description);
        }

        [Fact]
        public void Apply_SetsParameterDescription_FromSummaryTagsOfParameterBoundProperties()
        {
            var operation = new Operation
            {
                Parameters = new List<IParameter>() { new NonBodyParameter { Name = "Property" } },
                Responses = new Dictionary<string, Response>()
            };
            var filterContext = FilterContextFor(nameof(FakeController.AcceptsXmlAnnotatedTypeFromQuery));

            Subject().Apply(operation, filterContext);

            Assert.Equal("summary for Property", operation.Parameters.First().Description);
        }

        [Fact]
        public void Apply_OverwritesResponseDescriptionFromResponseTag_IfResponsePresent()
        {
            var operation = new Operation
            {
                Responses = new Dictionary<string, Response>
                {
                    { "200", new Response { Description = "Success", Schema = new Schema { Ref = "#/definitions/foo" } } },
                    { "400", new Response { Description = "Client Error", Schema = new Schema { Ref = "#/definitions/bar" } } }
                }
            };
            var filterContext = FilterContextFor(nameof(FakeController.AnnotatedWithXml));

            Subject().Apply(operation, filterContext);

            Assert.Equal("description for 200", operation.Responses["200"].Description);
            Assert.NotNull(operation.Responses["200"].Schema.Ref);
            Assert.Equal("description for 400", operation.Responses["400"].Description);
            Assert.NotNull(operation.Responses["400"].Schema.Ref);
        }

        [Fact]
        public void Apply_AddsResponseWithDescriptionFromResponseTag_IfResponseNotPresent()
        {
            var operation = new Operation
            {
                Responses = new Dictionary<string, Response>
                {
                    { "200", new Response { Description = "Success", Schema = new Schema { Ref = "#/definitions/foo" } } },
                }
            };
            var filterContext = FilterContextFor(nameof(FakeController.AnnotatedWithXml));

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "200", "400" }, operation.Responses.Keys.ToArray());
            Assert.Equal("description for 400", operation.Responses["400"].Description);
        }

        private OperationFilterContext FilterContextFor(string actionFixtureName)
        {
            var fakeProvider = new FakeApiDescriptionGroupCollectionProvider();
            var apiDescription = fakeProvider
                .Add("GET", "collection", actionFixtureName)
                .ApiDescriptionGroups.Items.First()
                .Items.First();

            return new OperationFilterContext(apiDescription, null, null);
        }

        private XmlCommentsOperationFilter Subject()
        {
            using (var xmlComments = File.OpenText(GetType().GetTypeInfo()
                    .Assembly.GetName().Name + ".xml"))
            {
                return new XmlCommentsOperationFilter(new XPathDocument(xmlComments));
            }
        }
    }
}