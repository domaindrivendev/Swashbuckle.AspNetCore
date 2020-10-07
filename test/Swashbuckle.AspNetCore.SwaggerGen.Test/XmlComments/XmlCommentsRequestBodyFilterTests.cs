﻿using System.IO;
using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Xunit;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsRequestBodyFilterTests
    {
        [Fact]
        public void Apply_SetsDescription_FromActionParamTag()
        {
            var requestbody = new OpenApiRequestBody();
            var parameterInfo = typeof(FakeControllerWithXmlComments)
                .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParamTags))
                .GetParameters()[0];
            var bodyParameterDescription = new ApiParameterDescription
            {
                ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo }
            };
            var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null);

            Subject().Apply(requestbody, filterContext);

            Assert.Equal("Description for param1", requestbody.Description);
        }

        [Fact]
        public void Apply_SetsDescription_FromUnderlyingGenericTypeActionParamTag()
        {
            var requestbody = new OpenApiRequestBody();
            var parameterInfo = typeof(FakeConstructedControllerWithXmlComments)
                .GetMethod(nameof(FakeConstructedControllerWithXmlComments.ActionWithParamTags))
                .GetParameters()[0];
            var bodyParameterDescription = new ApiParameterDescription
            {
                ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo }
            };
            var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null);

            Subject().Apply(requestbody, filterContext);

            Assert.Equal("Description for param1", requestbody.Description);
        }

        [Theory]
        [InlineData("Summary for StringProperty (Remarks for StringProperty)", true)]
        [InlineData("Summary for StringProperty", false)]
        public void Apply_SetsDescription_FromPropertySummaryAndRemarksTag(
            string expectedDescription,
            bool includeRemarksFromXmlComments)
        {
            var requestBody = new OpenApiRequestBody();
            var bodyParameterDescription = new ApiParameterDescription
            {
                ModelMetadata = ModelMetadataFactory.CreateForProperty(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringProperty))
            };
            var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null);

            Subject(includeRemarksFromXmlComments).Apply(requestBody, filterContext);

            Assert.Equal(expectedDescription, requestBody.Description);
        }

        private XmlCommentsRequestBodyFilter Subject(bool includeRemarksFromXmlComments = false)
        {
            using (var xmlComments = File.OpenText(typeof(FakeControllerWithXmlComments).Assembly.GetName().Name + ".xml"))
            {
                return new XmlCommentsRequestBodyFilter(new XPathDocument(xmlComments), includeRemarksFromXmlComments);
            }
        }
    }
}
