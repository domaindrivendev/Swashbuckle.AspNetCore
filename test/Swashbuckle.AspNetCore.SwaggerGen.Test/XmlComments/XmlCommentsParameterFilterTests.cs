using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
#if !NET10_0_OR_GREATER
using Swashbuckle.AspNetCore.TestSupport;
#endif

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class XmlCommentsParameterFilterTests
{
    [Theory]
    [InlineData(0, "Description for param1", "\"Example for \\u0022param1\\u0022\"")]
    [InlineData(1, "Description for param2", "\"http://test.com/?param1=1\\u0026param2=2\"")]
    [InlineData(2, "Description for param3 with empty example", "\"\"")]
    public void Apply_SetsDescriptionAndExample_FromActionParamTag(int p, string expectedDescription, string expectedExample)
    {
        var parameter = new OpenApiParameter { Schema = new OpenApiSchema { Type = JsonSchemaTypes.String } };
        var parameterInfo = typeof(FakeControllerWithXmlComments)
            .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParamTags))
            .GetParameters()[p];
        var apiParameterDescription = new ApiParameterDescription { };
        var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, null, parameterInfo: parameterInfo);

        Subject().Apply(parameter, filterContext);

        Assert.Equal(expectedDescription, parameter.Description);
        Assert.NotNull(parameter.Example);

        Assert.Equal(expectedExample, parameter.Example.ToJson());
    }

    [Theory]
    [InlineData(0, "Description for param1", "\"Example for \\u0022param1\\u0022\"")]
    [InlineData(1, "Description for param2", "\"http://test.com/?param1=1\\u0026param2=2\"")]
    [InlineData(2, "Description for param3 with empty example", "\"\"")]
    public void Apply_SetsDescriptionAndExample_FromUnderlyingGenericTypeActionParamTag(int p, string expectedDescription, string expectedExample)
    {
        var parameter = new OpenApiParameter { Schema = new OpenApiSchema { Type = JsonSchemaTypes.String } };
        var parameterInfo = typeof(FakeConstructedControllerWithXmlComments)
            .GetMethod(nameof(FakeConstructedControllerWithXmlComments.ActionWithParamTags))
            .GetParameters()[p];
        var apiParameterDescription = new ApiParameterDescription { };
        var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, null, parameterInfo: parameterInfo);

        Subject().Apply(parameter, filterContext);

        Assert.Equal(expectedDescription, parameter.Description);
        Assert.NotNull(parameter.Example);

        Assert.Equal(expectedExample, parameter.Example.ToJson());
    }

    [Fact]
    public void Apply_SetsDescriptionAndExample_FromPropertySummaryAndExampleTags()
    {
        var parameter = new OpenApiParameter { Schema = new OpenApiSchema { Type = JsonSchemaTypes.String, Description = "schema-level description" } };
        var propertyInfo = typeof(XmlAnnotatedType).GetProperty(nameof(XmlAnnotatedType.StringProperty));
        var apiParameterDescription = new ApiParameterDescription { };
        var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, null, propertyInfo: propertyInfo);

        Subject().Apply(parameter, filterContext);

        Assert.Equal("Summary for StringProperty", parameter.Description);
        Assert.Null(parameter.Schema.Description);
        Assert.NotNull(parameter.Example);
        Assert.Equal("\"Example for StringProperty\"", parameter.Example.ToJson());
    }

    [Fact]
    public void Apply_SetsDescriptionAndExample_FromUriTypePropertySummaryAndExampleTags()
    {
        var parameter = new OpenApiParameter { Schema = new OpenApiSchema { Type = JsonSchemaTypes.String, Description = "schema-level description" } };
        var propertyInfo = typeof(XmlAnnotatedType).GetProperty(nameof(XmlAnnotatedType.StringPropertyWithUri));
        var apiParameterDescription = new ApiParameterDescription { };
        var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, null, propertyInfo: propertyInfo);

        Subject().Apply(parameter, filterContext);

        Assert.Equal("Summary for StringPropertyWithUri", parameter.Description);
        Assert.Null(parameter.Schema.Description);
        Assert.NotNull(parameter.Example);

        Assert.Equal("\"https://test.com/a?b=1\\u0026c=2\"", parameter.Example.ToJson());
    }

    private static XmlCommentsParameterFilter Subject()
    {
        using var xml = File.OpenText(typeof(FakeControllerWithXmlComments).Assembly.GetName().Name + ".xml");
        var document = new XPathDocument(xml);
        var members = XmlCommentsDocumentHelper.CreateMemberDictionary(document);
        return new(members, new());
    }
}
