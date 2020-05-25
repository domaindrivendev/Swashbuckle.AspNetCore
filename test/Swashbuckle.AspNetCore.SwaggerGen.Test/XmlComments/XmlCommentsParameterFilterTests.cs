using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.TestSupport;
using System.IO;
using System.Xml.XPath;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsParameterFilterTests
    {
        [Fact]
        public void Apply_SetsDescription_FromActionParamTag()
        {
            var parameter = new OpenApiParameter();
            var parameterInfo = typeof(FakeControllerWithXmlComments)
                .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParamTags))
                .GetParameters()[0];
            var apiParameterDescription = new ApiParameterDescription { };
            var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, parameterInfo: parameterInfo);

            Subject().Apply(parameter, filterContext);

            Assert.Equal("Description for param1", parameter.Description);
        }

        [Fact]
        public void Apply_SetsDescription_FromUnderlyingGenericTypeActionParamTag()
        {
            var parameter = new OpenApiParameter();
            var parameterInfo = typeof(FakeConstructedControllerWithXmlComments)
                .GetMethod(nameof(FakeConstructedControllerWithXmlComments.ActionWithParamTags))
                .GetParameters()[0];
            var apiParameterDescription = new ApiParameterDescription { };
            var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, parameterInfo: parameterInfo);

            Subject().Apply(parameter, filterContext);

            Assert.Equal("Description for param1", parameter.Description);
        }

        [Fact]
        public void Apply_SetsDescription_FromPropertySummaryTag()
        {
            var parameter = new OpenApiParameter();
            var propertyInfo = typeof(XmlAnnotatedType).GetProperty(nameof(XmlAnnotatedType.StringProperty));
            var apiParameterDescription = new ApiParameterDescription { };
            var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, propertyInfo: propertyInfo);

            Subject().Apply(parameter, filterContext);

            Assert.Equal("Summary for StringProperty", parameter.Description);
        }

        [Theory]
        [InlineData(0, "boolean", null, true)]
        [InlineData(1, "integer", "int32", 27)]
        [InlineData(2, "integer", "int64", 4294967296L)]
        [InlineData(3, "number", "float", 1.23F)]
        [InlineData(4, "number", "double", 1.25D)]
        [InlineData(5, "integer", "int32", 2)]
        [InlineData(6, "string", "uuid", "1edab3d2-311a-4782-9ec9-a70d0478b82f")]
        [InlineData(7, "string", null, "Example for StringProperty")]
        [InlineData(8, "integer", "int32", null)]
        public void Apply_SetsExample_FromActionParamTag(
            int parameterIndex,
            string schemaType,
            string schemaFormat,
            object expectedValue)
        {
            var parameter = new OpenApiParameter
            {
                Schema = new OpenApiSchema { Type = schemaType, Format = schemaFormat }
            };

            var parameterInfo = typeof(FakeControllerWithXmlComments)
                .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithExampleParams))
                .GetParameters()[parameterIndex];
            var filterContext = new ParameterFilterContext(new ApiParameterDescription(), null, null, parameterInfo: parameterInfo);

            Subject().Apply(parameter, filterContext);

            if (expectedValue != null)
            {
                Assert.NotNull(parameter.Example);
                Assert.Equal(expectedValue, parameter.Example.GetType().GetProperty("Value").GetValue(parameter.Example));
            }
            else
            {
                Assert.Null(parameter.Example);
            }
        }

        private XmlCommentsParameterFilter Subject()
        {
            using (var xmlComments = File.OpenText(typeof(FakeControllerWithXmlComments).Assembly.GetName().Name + ".xml"))
            {
                return new XmlCommentsParameterFilter(new XPathDocument(xmlComments));
            }
        }
    }
}
