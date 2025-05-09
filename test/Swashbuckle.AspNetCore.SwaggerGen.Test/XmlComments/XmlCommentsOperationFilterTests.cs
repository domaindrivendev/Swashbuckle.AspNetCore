using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class XmlCommentsOperationFilterTests
{
    [Fact]
    public void Apply_SetsSummaryAndDescription_FromActionSummaryAndRemarksTags()
    {
        var operation = new OpenApiOperation();
        var methodInfo = typeof(FakeControllerWithXmlComments)
            .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithSummaryAndRemarksTags));
        var apiDescription = ApiDescriptionFactory.Create(methodInfo: methodInfo, groupName: "v1", httpMethod: "POST", relativePath: "resource");
        var filterContext = new OperationFilterContext(apiDescription, null, null, null, methodInfo);

        Subject().Apply(operation, filterContext);

        Assert.Equal("Summary for ActionWithSummaryAndRemarksTags", operation.Summary);
        Assert.Equal("Remarks for ActionWithSummaryAndRemarksTags", operation.Description);
    }

    [Fact]
    public void Apply_SetsSummaryAndDescription_FromUnderlyingGenericTypeActionSummaryAndRemarksTags()
    {
        var operation = new OpenApiOperation();
        var methodInfo = typeof(FakeConstructedControllerWithXmlComments)
            .GetMethod(nameof(FakeConstructedControllerWithXmlComments.ActionWithSummaryAndResponseTags));
        var apiDescription = ApiDescriptionFactory.Create(methodInfo: methodInfo, groupName: "v1", httpMethod: "POST", relativePath: "resource");
        var filterContext = new OperationFilterContext(apiDescription, null, null, null, methodInfo);

        Subject().Apply(operation, filterContext);

        Assert.Equal("Summary for ActionWithSummaryAndRemarksTags", operation.Summary);
        Assert.Equal("Remarks for ActionWithSummaryAndRemarksTags", operation.Description);
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
        var methodInfo = typeof(FakeControllerWithXmlComments)
            .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithResponseTags));
        var apiDescription = ApiDescriptionFactory.Create(
            methodInfo: methodInfo,
            groupName: "v1",
            httpMethod: "POST",
            relativePath: "resource",
            supportedResponseTypes:
            [
                new ApiResponseType { StatusCode = 200 },
                new ApiResponseType { StatusCode = 400 },
            ]);
        var filterContext = new OperationFilterContext(apiDescription, null, null, null, methodInfo: methodInfo);

        Subject().Apply(operation, filterContext);

        Assert.Equal(["200", "400", "default"], [.. operation.Responses.Keys]);
        Assert.Equal("Description for 200 response", operation.Responses["200"].Description);
        Assert.Equal("Description for 400 response", operation.Responses["400"].Description);
        Assert.Equal("Description for default response", operation.Responses["default"].Description);
    }

    private static XmlCommentsOperationFilter Subject()
    {
        using var xml = File.OpenText(typeof(FakeControllerWithXmlComments).Assembly.GetName().Name + ".xml");
        var document = new XPathDocument(xml);
        var members = XmlCommentsDocumentHelper.CreateMemberDictionary(document);
        return new(members, new());
    }
}
