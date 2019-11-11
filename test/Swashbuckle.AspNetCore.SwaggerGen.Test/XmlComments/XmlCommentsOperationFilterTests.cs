using System.Linq;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Reflection;
using System.IO;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Xunit;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsOperationFilterTests
    {
        [Fact]
        public void Apply_SetsSummaryAndDescription_FromActionSummaryAndRemarksTags()
        {
            var operation = new OpenApiOperation
            {
                Responses = new OpenApiResponses()
            };
            var filterContext = FilterContextFor
            (
                ApiDescriptionFactory.Create<FakeControllerWithXmlComments>(
                    c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "resource")
            );

            Subject().Apply(operation, filterContext);

            Assert.Equal("Summary for ActionWithNoParameters", operation.Summary);
            Assert.Equal("Remarks for ActionWithNoParameters", operation.Description);
        }

        [Fact]
        public void Apply_SetsParameterDescriptions_FromCorrespondingActionParamTags()
        {
            var operation = new OpenApiOperation
            {
                Parameters = new List<OpenApiParameter>
                {
                    new OpenApiParameter { Name = "param1" },
                    new OpenApiParameter { Name = "param2" },
                },
                Responses = new OpenApiResponses()
            };
            var filterContext = FilterContextFor
            (
                ApiDescriptionFactory.Create<FakeControllerWithXmlComments>(
                    c => nameof(c.ActionWithMultipleParameters),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions: new[]
                    {
                        new ApiParameterDescription { Name = "param1", Source = BindingSource.Query },
                        new ApiParameterDescription { Name = "param2", Source = BindingSource.Query }
                    })
            );

            Subject().Apply(operation, filterContext);

            Assert.Equal("Description for param1", operation.Parameters[0].Description);
            Assert.Equal("Description for param2", operation.Parameters[1].Description);
        }

        [Fact]
        public void Apply_SetsParameterDescriptions_FromCorrespondingPropertySummaryTags()
        {
            var operation = new OpenApiOperation
            {
                Parameters = new List<OpenApiParameter>
                {
                    new OpenApiParameter { Name = "BoolProperty" }
                },
                Responses = new OpenApiResponses()
            };
            var filterContext = FilterContextFor
            (
                ApiDescriptionFactory.Create<FakeControllerWithXmlComments>(
                    c => nameof(c.ActionWithNoParameters),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions: new[]
                    {
                        new ApiParameterDescription
                        {
                            Name = "BoolProperty",
                            Source = BindingSource.Query,
                            ModelMetadata = ModelMetadataFactory.CreateForProperty(typeof(XmlAnnotatedType), "BoolProperty")
                        }
                    })
            );

            Subject().Apply(operation, filterContext);

            Assert.Equal("Summary for BoolProperty", operation.Parameters[0].Description);
        }

        [Fact]
        public void Apply_SetsRequestBodyDescription_FromCorrespondingActionParamTags()
        {
            var operation = new OpenApiOperation
            {
                RequestBody = new OpenApiRequestBody(),
                Responses = new OpenApiResponses()
            };
            var filterContext = FilterContextFor
            (
                ApiDescriptionFactory.Create<FakeControllerWithXmlComments>(
                    c => nameof(c.ActionWithParamTag),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    parameterDescriptions: new []
                    {
                        new ApiParameterDescription { Name = "param", Source = BindingSource.Body }
                    })
            );

            Subject().Apply(operation, filterContext);

            Assert.Equal("Description for param", operation.RequestBody.Description);
        }

        [Fact]
        public void Apply_SetsResponseDescription_FromActionOrControllerResponseTags()
        {
            var operation = new OpenApiOperation
            {
                Responses = new OpenApiResponses
                {
                    { "200", new OpenApiResponse { Description = "Success" } },
                    { "400", new OpenApiResponse { Description = "Client Error" } },
                }
            };
            var filterContext = FilterContextFor
            (
                ApiDescriptionFactory.Create<FakeControllerWithXmlComments>(
                    c => nameof(c.ActionWithResponseTags),
                    groupName: "v1",
                    httpMethod: "POST",
                    relativePath: "resource",
                    supportedResponseTypes: new []
                    {
                        new ApiResponseType { StatusCode = 200 },
                        new ApiResponseType { StatusCode = 400 },
                    })
            );

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "200", "400", "default" }, operation.Responses.Keys.ToArray());
            Assert.Equal("Description for 200 response", operation.Responses["200"].Description);
            Assert.Equal("Description for 400 response", operation.Responses["400"].Description);
            Assert.Equal("Description for default response", operation.Responses["default"].Description);
        }

        private OperationFilterContext FilterContextFor(ApiDescription apiDescription)
        {
            return new OperationFilterContext(
                apiDescription,
                null,
                null,
                (apiDescription.ActionDescriptor as ControllerActionDescriptor).MethodInfo);
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