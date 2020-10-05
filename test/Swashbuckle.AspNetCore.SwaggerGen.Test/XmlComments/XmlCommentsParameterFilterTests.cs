using System.IO;
using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsParameterFilterTests
    {
        [Fact]
        public void Apply_SetsDescriptionAndExample_FromActionParamTag()
        {
            var parameter = new OpenApiParameter { Schema = new OpenApiSchema { Type = "string" } };
            var parameterInfo = typeof(FakeControllerWithXmlComments)
                .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParamTags))
                .GetParameters()[0];
            var apiParameterDescription = new ApiParameterDescription { };
            var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, parameterInfo: parameterInfo);

            Subject().Apply(parameter, filterContext);

            Assert.Equal("Description for param1", parameter.Description);
            Assert.Equal("Example for param1", ((OpenApiString)parameter.Example).Value);
        }

        [Fact]
        public void Apply_SetsDescriptionAndExample_FromUnderlyingGenericTypeActionParamTag()
        {
            var parameter = new OpenApiParameter { Schema = new OpenApiSchema { Type = "string" } };
            var parameterInfo = typeof(FakeConstructedControllerWithXmlComments)
                .GetMethod(nameof(FakeConstructedControllerWithXmlComments.ActionWithParamTags))
                .GetParameters()[0];
            var apiParameterDescription = new ApiParameterDescription { };
            var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, parameterInfo: parameterInfo);

            Subject().Apply(parameter, filterContext);

            Assert.Equal("Description for param1", parameter.Description);
            Assert.Equal("Example for param1", ((OpenApiString)parameter.Example).Value);
        }

        [Fact]
        public void Apply_SetsDescriptionAndExample_FromPropertySummaryAndExampleTags()
        {
            var parameter = new OpenApiParameter { Schema = new OpenApiSchema { Type = "string" } };
            var propertyInfo = typeof(XmlAnnotatedType).GetProperty(nameof(XmlAnnotatedType.StringProperty));
            var apiParameterDescription = new ApiParameterDescription { };
            var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, propertyInfo: propertyInfo);

            Subject().Apply(parameter, filterContext);

            Assert.Equal("Summary for StringProperty", parameter.Description);
            Assert.Equal("Example for StringProperty", ((OpenApiString)parameter.Example).Value);
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
