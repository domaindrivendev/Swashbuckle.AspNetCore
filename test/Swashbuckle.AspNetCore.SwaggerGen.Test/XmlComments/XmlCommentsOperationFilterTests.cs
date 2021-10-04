using System.Globalization;
using System.Linq;
using System.Xml.XPath;
using System.IO;
using Microsoft.OpenApi.Models;
using Xunit;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsOperationFilterTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("en-US")]
        [InlineData("ru-RU")]
        public void Apply_SetsSummaryAndDescription_FromActionSummaryAndRemarksTags(string cultureName)
        {
            var operation = new OpenApiOperation();
            var methodInfo = typeof(FakeControllerWithXmlComments)
                .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithSummaryAndRemarksTags));
            var apiDescription = ApiDescriptionFactory.Create(methodInfo: methodInfo, groupName: "v1", httpMethod: "POST", relativePath: "resource");
            var filterContext = new OperationFilterContext(apiDescription, null, null, methodInfo);

            Subject(cultureName).Apply(operation, filterContext);

            switch (cultureName)
            {
                case "ru-RU":
                    Assert.Equal("Summary для ActionWithSummaryAndRemarksTags с русской локализацией", operation.Summary);
                    Assert.Equal("Remarks для ActionWithSummaryAndRemarksTags с русской локализацией", operation.Description);
                    break;
                case "en-US":
                default:
                    Assert.Equal("Summary for ActionWithSummaryAndRemarksTags", operation.Summary);
                    Assert.Equal("Remarks for ActionWithSummaryAndRemarksTags", operation.Description);
                    break;
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("en-US")]
        [InlineData("ru-RU")]
        public void Apply_SetsSummaryAndDescription_FromUnderlyingGenericTypeActionSummaryAndRemarksTags(string cultureName)
        {
            var operation = new OpenApiOperation();
            var methodInfo = typeof(FakeConstructedControllerWithXmlComments)
                .GetMethod(nameof(FakeConstructedControllerWithXmlComments.ActionWithSummaryAndResponseTags));
            var apiDescription = ApiDescriptionFactory.Create(methodInfo: methodInfo, groupName: "v1", httpMethod: "POST", relativePath: "resource");
            var filterContext = new OperationFilterContext(apiDescription, null, null, methodInfo);

            Subject(cultureName).Apply(operation, filterContext);

            switch (cultureName)
            {
                case "ru-RU":
                    Assert.Equal("Summary для ActionWithSummaryAndRemarksTags с русской локализацией", operation.Summary);
                    Assert.Equal("Remarks для ActionWithSummaryAndRemarksTags с русской локализацией", operation.Description);
                    break;
                case "en-US":
                default:
                    Assert.Equal("Summary for ActionWithSummaryAndRemarksTags", operation.Summary);
                    Assert.Equal("Remarks for ActionWithSummaryAndRemarksTags", operation.Description);
                    break;
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("en-US")]
        [InlineData("ru-RU")]
        public void Apply_SetsResponseDescription_FromActionOrControllerResponseTags(string cultureName)
        {
            var operation = new OpenApiOperation
            {
                Responses = new OpenApiResponses
                {
                    { "200", new OpenApiResponse { Description = "Success" } },
                    { "400", new OpenApiResponse { Description = "Client Error" } },
                }
            };
            var methodInfo = typeof(FakeControllerWithXmlComments)
                .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithResponseTags));
            var apiDescription = ApiDescriptionFactory.Create(
                methodInfo: methodInfo,
                groupName: "v1",
                httpMethod: "POST",
                relativePath: "resource",
                supportedResponseTypes: new[]
                {
                    new ApiResponseType { StatusCode = 200 },
                    new ApiResponseType { StatusCode = 400 },
                });
            var filterContext = new OperationFilterContext(apiDescription, null, null, methodInfo: methodInfo);

            Subject(cultureName).Apply(operation, filterContext);

            Assert.Equal(new[] { "200", "400", "default" }, operation.Responses.Keys.ToArray());
            switch (cultureName)
            {
                case "ru-RU":
                    Assert.Equal("Описание для 200 ответа", operation.Responses["200"].Description);
                    Assert.Equal("Описание для 400 ответа", operation.Responses["400"].Description);
                    Assert.Equal("Описание для ответа по умолчанию", operation.Responses["default"].Description);
                    break;
                case null:
                    Assert.Equal("Description for 200 response", operation.Responses["200"].Description);
                    Assert.Equal("Description for 400 response", operation.Responses["400"].Description);
                    Assert.Equal("Description for default response", operation.Responses["default"].Description);
                    break;
                default:
                    Assert.Equal("Success", operation.Responses["200"].Description);
                    Assert.Equal("Client Error", operation.Responses["400"].Description);
                    Assert.Equal("Description for default response", operation.Responses["default"].Description);
                    break;
            }
        }

        private XmlCommentsOperationFilter Subject(string cultureName = null)
        {
            using (var xmlComments = File.OpenText(typeof(FakeControllerWithXmlComments).Assembly.GetName().Name + ".xml"))
            {
                var culture = cultureName == null ? null : new CultureInfo(cultureName);
                return new XmlCommentsOperationFilter(new XPathDocument(xmlComments), culture);
            }
        }
    }
}