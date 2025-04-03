using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class XmlCommentsRequestBodyFilterTests
{
    [Fact]
    public void Apply_SetsDescriptionAndExample_FromActionParamTag()
    {
        var requestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = JsonSchemaTypes.String } }
            }
        };
        var parameterInfo = typeof(FakeControllerWithXmlComments)
            .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParamTags))
            .GetParameters()[0];
        var bodyParameterDescription = new ApiParameterDescription
        {
            ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo }
        };
        var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null, null);

        Subject().Apply(requestBody, filterContext);

        Assert.Equal("Description for param1", requestBody.Description);
        Assert.NotNull(requestBody.Content["application/json"].Example);

        Assert.Equal("\"Example for \\u0022param1\\u0022\"", requestBody.Content["application/json"].Example.ToJson());
    }

    [Fact]
    public void Apply_SetsDescriptionAndExample_FromUnderlyingGenericTypeActionParamTag()
    {
        var requestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = JsonSchemaTypes.String } }
            }
        };
        var parameterInfo = typeof(FakeConstructedControllerWithXmlComments)
            .GetMethod(nameof(FakeConstructedControllerWithXmlComments.ActionWithParamTags))
            .GetParameters()[0];
        var bodyParameterDescription = new ApiParameterDescription
        {
            ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo }
        };
        var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null, null);

        Subject().Apply(requestBody, filterContext);

        Assert.Equal("Description for param1", requestBody.Description);
        Assert.NotNull(requestBody.Content["application/json"].Example);

        Assert.Equal("\"Example for \\u0022param1\\u0022\"", requestBody.Content["application/json"].Example.ToJson());
    }

    [Fact]
    public void Apply_SetsDescriptionAndExample_FromPropertySummaryAndExampleTags()
    {
        var requestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = JsonSchemaTypes.String } }
            }
        };
        var bodyParameterDescription = new ApiParameterDescription
        {
            ModelMetadata = ModelMetadataFactory.CreateForProperty(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringProperty))
        };
        var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null, null);

        Subject().Apply(requestBody, filterContext);

        Assert.Equal("Summary for StringProperty", requestBody.Description);
        Assert.NotNull(requestBody.Content["application/json"].Example);
        Assert.Equal("\"Example for StringProperty\"", requestBody.Content["application/json"].Example.ToJson());
    }


    [Fact]
    public void Apply_SetsDescriptionAndExample_FromUriTypePropertySummaryAndExampleTags()
    {
        var requestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = JsonSchemaTypes.String } }
            }
        };
        var bodyParameterDescription = new ApiParameterDescription
        {
            ModelMetadata = ModelMetadataFactory.CreateForProperty(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringPropertyWithUri))
        };
        var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null, null);

        Subject().Apply(requestBody, filterContext);

        Assert.Equal("Summary for StringPropertyWithUri", requestBody.Description);
        Assert.NotNull(requestBody.Content["application/json"].Example);

        Assert.Equal("\"https://test.com/a?b=1\\u0026c=2\"", requestBody.Content["application/json"].Example.ToJson());
    }

    [Fact]
    public void Apply_SetsDescription_ForParameterFromBody()
    {
        var requestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = JsonSchemaTypes.String } }
            }
        };
        var parameterInfo = typeof(FakeControllerWithXmlComments)
            .GetMethod(nameof(FakeControllerWithXmlComments.PostBody))
            .GetParameters()[0];
        var bodyParameterDescription = new ApiParameterDescription
        {
            ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo }
        };
        var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null, null);

        Subject().Apply(requestBody, filterContext);

        Assert.Equal("Parameter from JSON body", requestBody.Description);
    }

    [Fact]
    public void Apply_SetsDescription_ForParameterFromForm()
    {
        var parameterInfo = typeof(FakeControllerWithXmlComments)
            .GetMethod(nameof(FakeControllerWithXmlComments.PostForm))
            .GetParameters()[0];

        var requestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaTypes.String,
                        Properties = new Dictionary<string, IOpenApiSchema>()
                        {
                            [parameterInfo.Name] = new OpenApiSchema()
                        }
                    },
                }
            }
        };

        var bodyParameterDescription = new ApiParameterDescription
        {
            ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo },
            Name = parameterInfo.Name,
            Source = BindingSource.Form
        };
        var filterContext = new RequestBodyFilterContext(null, [bodyParameterDescription], null, null, null);

        Subject().Apply(requestBody, filterContext);

        Assert.Equal("Parameter from form body", requestBody.Content["multipart/form-data"].Schema.Properties[parameterInfo.Name].Description);
    }

    private static XmlCommentsRequestBodyFilter Subject()
    {
        using var xml = File.OpenText(typeof(FakeControllerWithXmlComments).Assembly.GetName().Name + ".xml");
        var document = new XPathDocument(xml);
        var members = XmlCommentsDocumentHelper.CreateMemberDictionary(document);
        return new(members, new());
    }
}
