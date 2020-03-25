using System.IO;
using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsRequestBodyFilterTests
    {
        [Fact]
        public void Apply_SetsDescription_FromPropertySummaryTag()
        {
            var requestBody = new OpenApiRequestBody();
            var bodyParameterDescription = new ApiParameterDescription
            {
                ModelMetadata = ModelMetadataFactory.CreateForProperty(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringProperty))
            };
            var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null);

            Subject().Apply(requestBody, filterContext);

            Assert.Equal("Summary for StringProperty", requestBody.Description);
        }

        [Fact]
        public void Apply_SetsDescription_FromActionParamTag()
        {
            var requestbody = new OpenApiRequestBody();
            var parameterInfo = typeof(FakeControllerWithXmlComments)
                .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParameter))
                .GetParameters()[0];
            var bodyParameterDescription = new ApiParameterDescription
            {
                ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo }
            };
            var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null);

            Subject().Apply(requestbody, filterContext);

            Assert.Equal("Description for param", requestbody.Description);
        }

        [Fact]
        public void Apply_SetsDescription_FromUnderlyingGenericTypeActionParamTag()
        {
            var requestbody = new OpenApiRequestBody();
            var parameterInfo = typeof(FakeConstructedControllerWithXmlComments)
                .GetMethod(nameof(FakeConstructedControllerWithXmlComments.ActionWithGenericTypeParameter))
                .GetParameters()[0];
            var bodyParameterDescription = new ApiParameterDescription
            {
                ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo }
            };
            var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null);

            Subject().Apply(requestbody, filterContext);

            Assert.Equal("Description for param", requestbody.Description);
        }


        private XmlCommentsRequestBodyFilter Subject()
        {
            using (var xmlComments = File.OpenText(GetType().Assembly.GetName().Name + ".xml"))
            {
                return new XmlCommentsRequestBodyFilter(new XPathDocument(xmlComments));
            }
        }
    }
}
