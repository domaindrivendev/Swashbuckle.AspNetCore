using System.Globalization;
using System.IO;
using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.TestSupport;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsParameterFilterTests
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
            var parameter = new OpenApiParameter { Schema = new OpenApiSchema { Type = "string" } };
            var parameterInfo = typeof(FakeControllerWithXmlComments)
                .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParamTags))?
                .GetParameters()[0];
            var apiParameterDescription = new ApiParameterDescription { };
            var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, parameterInfo: parameterInfo);

            Subject(cultureName).Apply(parameter, filterContext);

            Assert.Equal(expectedDescription, parameter.Description);
            Assert.NotNull(parameter.Example);
            Assert.Equal(expectedExample, parameter.Example.ToJson());
        }

        [Theory]
        [InlineData("Description for param2", null)]
        [InlineData("Description for param2", "en-US")]
        [InlineData("Описание для param2", "ru-RU")]
        public void Apply_SetsDescriptionAndExample_FromUriTypeActionParamTag(
            string expectedDescription,
            string cultureName)
        {
            var parameter = new OpenApiParameter { Schema = new OpenApiSchema { Type = "string" } };
            var parameterInfo = typeof(FakeControllerWithXmlComments)
                .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParamTags))?
                .GetParameters()[1];
            var apiParameterDescription = new ApiParameterDescription { };
            var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, parameterInfo: parameterInfo);

            Subject(cultureName).Apply(parameter, filterContext);

            Assert.Equal(expectedDescription, parameter.Description);
            Assert.NotNull(parameter.Example);
            Assert.Equal("\"http://test.com/?param1=1&param2=2\"", parameter.Example.ToJson());
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
            var parameter = new OpenApiParameter { Schema = new OpenApiSchema { Type = "string" } };
            var parameterInfo = typeof(FakeConstructedControllerWithXmlComments)
                .GetMethod(nameof(FakeConstructedControllerWithXmlComments.ActionWithParamTags))?
                .GetParameters()[0];
            var apiParameterDescription = new ApiParameterDescription { };
            var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, parameterInfo: parameterInfo);

            Subject(cultureName).Apply(parameter, filterContext);

            Assert.Equal(expectedDescription, parameter.Description);
            Assert.NotNull(parameter.Example);
            Assert.Equal(expectedExample, parameter.Example.ToJson());
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
            var parameter = new OpenApiParameter { Schema = new OpenApiSchema { Type = "string", Description = "schema-level description" } };
            var propertyInfo = typeof(XmlAnnotatedType).GetProperty(nameof(XmlAnnotatedType.StringProperty));
            var apiParameterDescription = new ApiParameterDescription { };
            var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, propertyInfo: propertyInfo);

            Subject(cultureName).Apply(parameter, filterContext);

            Assert.Equal(expectedDescription, parameter.Description);
            Assert.Null(parameter.Schema.Description);
            Assert.NotNull(parameter.Example);
            Assert.Equal(expectedExample, parameter.Example.ToJson());
        }

        [Theory]
        [InlineData("Summary for StringPropertyWithUri", null)]
        [InlineData("Summary for StringPropertyWithUri", "en-US")]
        [InlineData("Summary для StringPropertyWithUri", "ru-RU")]
        public void Apply_SetsDescriptionAndExample_FromUriTypePropertySummaryAndExampleTags(
            string expectedDescription,
            string cultureName)
        {
            var parameter = new OpenApiParameter { Schema = new OpenApiSchema { Type = "string", Description = "schema-level description" } };
            var propertyInfo = typeof(XmlAnnotatedType).GetProperty(nameof(XmlAnnotatedType.StringPropertyWithUri));
            var apiParameterDescription = new ApiParameterDescription { };
            var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, propertyInfo: propertyInfo);

            Subject(cultureName).Apply(parameter, filterContext);

            Assert.Equal(expectedDescription, parameter.Description);
            Assert.Null(parameter.Schema.Description);
            Assert.NotNull(parameter.Example);
            Assert.Equal("\"https://test.com/a?b=1&c=2\"", parameter.Example.ToJson());
        }

        private XmlCommentsParameterFilter Subject(string cultureName = null)
        {
            using (var xmlComments = File.OpenText(typeof(FakeControllerWithXmlComments).Assembly.GetName().Name + ".xml"))
            {
                var culture = cultureName == null ? null : new CultureInfo(cultureName);
                return new XmlCommentsParameterFilter(new XPathDocument(xmlComments), culture);
            }
        }
    }
}
