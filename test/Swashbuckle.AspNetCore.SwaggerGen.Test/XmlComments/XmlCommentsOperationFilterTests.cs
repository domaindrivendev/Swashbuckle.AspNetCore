using System.Linq;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Reflection;
using System.IO;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsOperationFilterTests
    {
        [Fact]
        public void Apply_SetsSummaryAndDescription_FromSummaryAndRemarksTags()
        {
            var operation = new OpenApiOperation
            {
                Responses = new OpenApiResponses()
            };
            var filterContext = FilterContextFor(nameof(XmlAnnotatedController.XmlAnnotatedAction));

            Subject().Apply(operation, filterContext);

            Assert.Equal("summary for AnnotatedWithXml", operation.Summary);
            Assert.Equal("remarks for AnnotatedWithXml", operation.Description);
        }

        [Fact]
        public void Apply_SetsParameterDescriptions_FromParamTags()
        {
            var operation = new OpenApiOperation
            {
                Parameters = new List<OpenApiParameter>
                {
                    new OpenApiParameter { Name = "param1" }, 
                    new OpenApiParameter { Name = "param2" }, 
                    new OpenApiParameter { Name = "Param-3" } 
                },
                Responses = new OpenApiResponses(),
                RequestBody = new OpenApiRequestBody()
            };
            var filterContext = FilterContextFor(nameof(XmlAnnotatedController.XmlAnnotatedAction));

            Subject().Apply(operation, filterContext);

            Assert.Equal("description for param1", operation.Parameters[0].Description);
            Assert.Equal("description for param2", operation.Parameters[1].Description);
            Assert.Equal("description for param3", operation.Parameters[2].Description);
            Assert.Equal("description for param4", operation.RequestBody.Description);
        }

        [Fact]
        public void Apply_SetsParameterDescription_FromSummaryTagsOfParameterBoundProperties()
        {
            var operation = new OpenApiOperation
            {
                Parameters = new List<OpenApiParameter>() { new OpenApiParameter { Name = "StringProperty" } },
                Responses = new OpenApiResponses()
            };
            var filterContext = FilterContextFor(nameof(XmlAnnotatedController.AcceptsXmlAnnotatedTypeFromQuery));

            Subject().Apply(operation, filterContext);

            Assert.Equal("summary for StringProperty", operation.Parameters.First().Description);
        }

        [Fact]
        public void Apply_SetsResponseDescription_IfActionOrControllerHasCorrespondingResponseTag()
        {
            var operation = new OpenApiOperation
            {
                Responses = new OpenApiResponses
                {
                    { "200", new OpenApiResponse { Description = "Success" } },
                    { "400", new OpenApiResponse { Description = "Client Error" } } 
                }
            };
            var filterContext = FilterContextFor(nameof(XmlAnnotatedController.XmlAnnotatedAction));

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "200", "400" }, operation.Responses.Keys.ToArray());
            Assert.Equal("controller-level description for 400", operation.Responses["400"].Description);
            Assert.Equal("action-level description for 200", operation.Responses["200"].Description);
        }

        [Fact]
        public void Apply_AddsResponsesWithDescriptions_IfActionOrControllerHasResponseTags()
        {
            var operation = new OpenApiOperation
            {
                Responses = new OpenApiResponses
                {
                    { "default", new OpenApiResponse { Description = "Unexpected Error" } } 
                }
            };
            var filterContext = FilterContextFor(nameof(XmlAnnotatedController.XmlAnnotatedAction));

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "default", "400", "200" }, operation.Responses.Keys.ToArray());
            Assert.Equal("controller-level description for 400", operation.Responses["400"].Description);
            Assert.Equal("action-level description for 200", operation.Responses["200"].Description);
        }

        private OperationFilterContext FilterContextFor(string actionFixtureName)
        {
            var fakeProvider = new FakeApiDescriptionGroupCollectionProvider();
            var apiDescription = fakeProvider
                .Add("GET", "collection", actionFixtureName, typeof(XmlAnnotatedController))
                .ApiDescriptionGroups.Items.First()
                .Items.First();

            var methodInfo = (apiDescription.ActionDescriptor as ControllerActionDescriptor).MethodInfo;

            return new OperationFilterContext(apiDescription, null, null, methodInfo);
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