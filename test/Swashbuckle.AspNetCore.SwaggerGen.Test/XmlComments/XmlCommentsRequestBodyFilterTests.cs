using System.IO;
using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Xunit;
using Swashbuckle.AspNetCore.TestSupport;
using System.Collections.Generic;
using System.Globalization;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsRequestBodyFilterTests
    {
        [Theory]
        [InlineData("Description for param1", "\"Example for param1\"", null)]
        [InlineData("Description for param1", "\"Example for param1\"", "en-US")]
        [InlineData("Описание для param1", "\"Пример для param1\"", "ru-RU")]
        public void Apply_SetsDescriptionAndExample_FromActionParamTag(
            string expectedDescription,
            string expectedExample,
            string cultureName)
        {
            var requestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = "string" } }
                }
            };
            var parameterInfo = typeof(FakeControllerWithXmlComments)
                .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParamTags))?
                .GetParameters()[0];
            var bodyParameterDescription = new ApiParameterDescription
            {
                ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo }
            };
            var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null);

            Subject(cultureName).Apply(requestBody, filterContext);

            Assert.Equal(expectedDescription, requestBody.Description);
            Assert.NotNull(requestBody.Content["application/json"].Example);
            Assert.Equal(expectedExample, requestBody.Content["application/json"].Example.ToJson());
        }

        [Theory]
        [InlineData("Description for param1", "\"Example for param1\"", null)]
        [InlineData("Description for param1", "\"Example for param1\"", "en-US")]
        [InlineData("Описание для param1", "\"Пример для param1\"", "ru-RU")]
        public void Apply_SetsDescriptionAndExample_FromUnderlyingGenericTypeActionParamTag(
            string expectedDescription,
            string expectedExample,
            string cultureName)
        {
            var requestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = "string" } }
                }
            };
            var parameterInfo = typeof(FakeConstructedControllerWithXmlComments)
                .GetMethod(nameof(FakeConstructedControllerWithXmlComments.ActionWithParamTags))?
                .GetParameters()[0];
            var bodyParameterDescription = new ApiParameterDescription
            {
                ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo }
            };
            var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null);

            Subject(cultureName).Apply(requestBody, filterContext);

            Assert.Equal(expectedDescription, requestBody.Description);
            Assert.NotNull(requestBody.Content["application/json"].Example);
            Assert.Equal(expectedExample, requestBody.Content["application/json"].Example.ToJson());
        }

        [Theory]
        [InlineData("Summary for StringProperty", "\"Example for StringProperty\"", null)]
        [InlineData("Summary for StringProperty", "\"Example for StringProperty\"", "en-US")]
        [InlineData("Summary для StringProperty", "\"Пример для StringProperty\"", "ru-RU")]
        public void Apply_SetsDescriptionAndExample_FromPropertySummaryAndExampleTags(
            string expectedDescription,
            string expectedExample,
            string cultureName)
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

            Subject(cultureName).Apply(requestBody, filterContext);

            Assert.Equal(expectedDescription, requestBody.Description);
            Assert.NotNull(requestBody.Content["application/json"].Example);
            Assert.Equal(expectedExample, requestBody.Content["application/json"].Example.ToJson());
        }

        [Theory]
        [InlineData("Summary for StringPropertyWithUri", null)]
        [InlineData("Summary for StringPropertyWithUri", "en-US")]
        [InlineData("Summary для StringPropertyWithUri", "ru-RU")]
        public void Apply_SetsDescriptionAndExample_FromUriTypePropertySummaryAndExampleTags(
            string expectedDescription,
            string cultureName)
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
                ModelMetadata = ModelMetadataFactory.CreateForProperty(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringPropertyWithUri))
            };
            var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null);

            Subject(cultureName).Apply(requestBody, filterContext);

            Assert.Equal(expectedDescription, requestBody.Description);
            Assert.NotNull(requestBody.Content["application/json"].Example);
            Assert.Equal("\"https://test.com/a?b=1&c=2\"", requestBody.Content["application/json"].Example.ToJson());
        }

        private XmlCommentsRequestBodyFilter Subject(string cultureName = null)
        {
            using (var xmlComments = File.OpenText(typeof(FakeControllerWithXmlComments).Assembly.GetName().Name + ".xml"))
            {
                var culture = cultureName == null ? null : new CultureInfo(cultureName);
                return new XmlCommentsRequestBodyFilter(new XPathDocument(xmlComments), culture);
            }
        }
    }
}
