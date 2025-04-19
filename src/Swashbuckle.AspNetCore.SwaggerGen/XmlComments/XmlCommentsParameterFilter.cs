using System.Reflection;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class XmlCommentsParameterFilter(IReadOnlyDictionary<string, XPathNavigator> xmlDocMembers, SwaggerGeneratorOptions options) : IParameterFilter
{
    private readonly IReadOnlyDictionary<string, XPathNavigator> _xmlDocMembers = xmlDocMembers;
    private readonly SwaggerGeneratorOptions _options = options;

    public void Apply(IOpenApiParameter parameter, ParameterFilterContext context)
    {
        if (context.PropertyInfo != null)
        {
            ApplyPropertyTags(parameter, context);
        }
        else if (context.ParameterInfo != null)
        {
            ApplyParamTags(parameter, context);
        }
    }

    private void ApplyPropertyTags(IOpenApiParameter parameter, ParameterFilterContext context)
    {
        var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(context.PropertyInfo);

        if (!_xmlDocMembers.TryGetValue(propertyMemberName, out var propertyNode)) return;

        var summaryNode = propertyNode.SelectFirstChild("summary");
        if (summaryNode != null)
        {
            parameter.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml, _options?.XmlCommentEndOfLine);
            parameter.Schema.Description = null; // No need to duplicate
        }

        if (parameter is OpenApiParameter concrete)
        {
            var exampleNode = propertyNode.SelectFirstChild("example");
            if (exampleNode != null)
            {
                concrete.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, parameter.Schema, exampleNode.ToString());
            }
        }
    }

    private void ApplyParamTags(IOpenApiParameter parameter, ParameterFilterContext context)
    {
        if (context.ParameterInfo.Member is not MethodInfo methodInfo)
        {
            return;
        }

        // If method is from a constructed generic type, look for comments from the generic type method
        var targetMethod = methodInfo.DeclaringType.IsConstructedGenericType
            ? methodInfo.GetUnderlyingGenericTypeMethod()
            : methodInfo;

        if (targetMethod == null) return;

        var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(targetMethod);

        if (!_xmlDocMembers.TryGetValue(methodMemberName, out var propertyNode))
        {
            return;
        }

        XPathNavigator paramNode = propertyNode.SelectFirstChildWithAttribute("param", "name", context.ParameterInfo.Name);

        if (paramNode != null)
        {
            parameter.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml, _options?.XmlCommentEndOfLine);

            if (parameter is OpenApiParameter concrete)
            {
                var example = paramNode.GetAttribute("example");
                if (!string.IsNullOrEmpty(example))
                {
                    concrete.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, parameter.Schema, example);
                }
            }
        }
    }
}
