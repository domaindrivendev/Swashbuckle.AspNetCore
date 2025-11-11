using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerGen.Test;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.Benchmarks;

[MemoryDiagnoser]
public class XmlCommentsBenchmark
{
    private XmlCommentsDocumentFilter _documentFilter;
    private OpenApiDocument _document;
    private DocumentFilterContext _documentFilterContext;

    private XmlCommentsOperationFilter _operationFilter;
    private OpenApiOperation _operation;
    private OperationFilterContext _operationFilterContext;

    private XmlCommentsParameterFilter _parameterFilter;
    private OpenApiParameter _parameter;
    private ParameterFilterContext _parameterFilterContext;

    private XmlCommentsRequestBodyFilter _requestBodyFilter;
    private OpenApiRequestBody _requestBody;
    private RequestBodyFilterContext _requestBodyFilterContext;

    private const int AddMemberCount = 10_000;

    [GlobalSetup]
    public void Setup()
    {
        // Load XML
        XmlDocument xmlDocument;
        using (var xmlComments = File.OpenText($"{typeof(FakeControllerWithXmlComments).Assembly.GetName().Name}.xml"))
        {
            xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlComments);
        }

        // Append dummy members to XML document
        XPathNavigator navigator = xmlDocument.CreateNavigator()!;
        navigator.MoveToRoot();
        navigator.MoveToChild("doc", string.Empty);
        navigator.MoveToChild("members", string.Empty);

        for (int i = 0; i < AddMemberCount; i++)
        {
            navigator.PrependChild(@$"<member name=""benchmark_{i}""></member>");
        }

        using var xmlStream = new MemoryStream();
        xmlDocument.Save(xmlStream);
        xmlStream.Seek(0, SeekOrigin.Begin);

        var xPathDocument = new XPathDocument(xmlStream);
        var members = XmlCommentsDocumentHelper.CreateMemberDictionary(xPathDocument);

        // Document
        _document = new OpenApiDocument();
        _documentFilterContext = new DocumentFilterContext(
            [
                new ApiDescription
                {
                    ActionDescriptor = new ControllerActionDescriptor
                    {
                        ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                        ControllerName = nameof(FakeControllerWithXmlComments),
                    },
                },
                new ApiDescription
                {
                    ActionDescriptor = new ControllerActionDescriptor
                    {
                        ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                        ControllerName = nameof(FakeControllerWithXmlComments),
                    },
                },
            ],
            null,
            null);

        _documentFilter = new XmlCommentsDocumentFilter(members, new());

        // Operation
        _operation = new OpenApiOperation();
        var methodInfo = typeof(FakeConstructedControllerWithXmlComments)
            .GetMethod(nameof(FakeConstructedControllerWithXmlComments.ActionWithSummaryAndResponseTags));

        var apiDescription = ApiDescriptionFactory.Create(methodInfo: methodInfo, groupName: "v1", httpMethod: "POST", relativePath: "resource");
        _operationFilterContext = new OperationFilterContext(apiDescription, null, null, null, methodInfo);
        _operationFilter = new XmlCommentsOperationFilter(members, new());

        // Parameter
        _parameter = new()
        {
            Schema = new OpenApiSchema()
            {
                Type = JsonSchemaTypes.String,
                Description = "schema-level description",
            },
        };

        var propertyInfo = typeof(XmlAnnotatedType).GetProperty(nameof(XmlAnnotatedType.StringProperty));
        var apiParameterDescription = new ApiParameterDescription();
        var xmlDocMembers = XmlCommentsDocumentHelper.CreateMemberDictionary(xPathDocument);
        _parameterFilterContext = new ParameterFilterContext(apiParameterDescription, null, null, null, propertyInfo: propertyInfo);
        _parameterFilter = new XmlCommentsParameterFilter(xmlDocMembers, new());

        // Request Body
        _requestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, IOpenApiMediaType>()
            {
                ["application/json"] = new OpenApiMediaType()
                {
                    Schema = new OpenApiSchema()
                    {
                        Type = JsonSchemaTypes.String,
                    },
                },
            },
        };
        var parameterInfo = typeof(FakeControllerWithXmlComments)
            .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParamTags))!
            .GetParameters()[0];

        var bodyParameterDescription = new ApiParameterDescription
        {
            ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo }
        };
        _requestBodyFilterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null, null);
        _requestBodyFilter = new XmlCommentsRequestBodyFilter(xmlDocMembers, new());
    }

    [Benchmark]
    public void Document()
        => _documentFilter.Apply(_document, _documentFilterContext);

    [Benchmark]
    public void Operation()
        => _operationFilter.Apply(_operation, _operationFilterContext);

    [Benchmark]
    public void Parameter()
        => _parameterFilter.Apply(_parameter, _parameterFilterContext);

    [Benchmark]
    public void RequestBody()
        => _requestBodyFilter.Apply(_requestBody, _requestBodyFilterContext);
}
