using System.IO;
using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Xunit;
using Swashbuckle.AspNetCore.TestSupport;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsRequestBodyFilterTests
    {
        [Fact]
        public void Apply_SetsDescriptionAndExample_FromActionParamTag()
        {
            var requestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = "string" } }
                }
            };
            var parameterInfo = typeof(FakeControllerWithXmlComments)
                .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParamTags))
                .GetParameters()[0];
            var bodyParameterDescription = new ApiParameterDescription
            {
                ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo }
            };
            var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null);

            Subject().Apply(requestBody, filterContext);

            Assert.Equal("Description for param1", requestBody.Description);
            Assert.NotNull(requestBody.Content["application/json"].Example);
            Assert.Equal("\"Example for param1\"", requestBody.Content["application/json"].Example.ToJson());
        }

        [Fact]
        public void Apply_SetsDescriptionAndExample_FromUnderlyingGenericTypeActionParamTag()
        {
            var requestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = "string" } }
                }
            };
            var parameterInfo = typeof(FakeConstructedControllerWithXmlComments)
                .GetMethod(nameof(FakeConstructedControllerWithXmlComments.ActionWithParamTags))
                .GetParameters()[0];
            var bodyParameterDescription = new ApiParameterDescription
            {
                ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo }
            };
            var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null);

            Subject().Apply(requestBody, filterContext);

            Assert.Equal("Description for param1", requestBody.Description);
            Assert.NotNull(requestBody.Content["application/json"].Example);
            Assert.Equal("\"Example for param1\"", requestBody.Content["application/json"].Example.ToJson());
        }

        [Fact]
        public void Apply_SetsDescriptionAndExample_FromPropertySummaryAndExampleTags()
        {
            var requestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = "string" } }
                }
            };
            var bodyParameterDescription = new ApiParameterDescription
            {
                ModelMetadata = ModelMetadataFactory.CreateForProperty(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringProperty))
            };
            var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null);

            Subject().Apply(requestBody, filterContext);

            Assert.Equal("Summary for StringProperty", requestBody.Description);
            Assert.NotNull(requestBody.Content["application/json"].Example);
            Assert.Equal("\"Example for StringProperty\"", requestBody.Content["application/json"].Example.ToJson());
        }

        private XmlCommentsRequestBodyFilter Subject()
        {
            using (var xmlComments = File.OpenText(typeof(FakeControllerWithXmlComments).Assembly.GetName().Name + ".xml"))
            {
                return new XmlCommentsRequestBodyFilter(new XPathDocument(xmlComments));
            }
        }
    }
}
