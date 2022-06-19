using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.XPath;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsParameterFilter : IParameterFilter
    {
        private readonly IReadOnlyDictionary<string, XPathNavigator> _xmlDocMembers;

        public XmlCommentsParameterFilter(XPathDocument xmlDoc) : this(XmlCommentsDocumentHelper.GetMemberDictionary(xmlDoc))
        {
        }

        public XmlCommentsParameterFilter(IReadOnlyDictionary<string, XPathNavigator> xmlDocMembers)
        {
            _xmlDocMembers = xmlDocMembers;
        }

        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
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

        private void ApplyPropertyTags(OpenApiParameter parameter, ParameterFilterContext context)
        {
            var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(context.PropertyInfo);

            if (!_xmlDocMembers.TryGetValue(propertyMemberName, out var propertyNode)) return;

            var summaryNode = propertyNode.SelectSingleNode("summary");
            if (summaryNode != null)
            {
                parameter.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
                parameter.Schema.Description = null; // no need to duplicate
            }

            var exampleNode = propertyNode.SelectSingleNode("example");
            if (exampleNode == null) return;

            var exampleAsJson = (parameter.Schema?.ResolveType(context.SchemaRepository) == "string")
                ? $"\"{exampleNode.ToString()}\""
                : exampleNode.ToString();

            parameter.Example = OpenApiAnyFactory.CreateFromJson(exampleAsJson);
        }

        private void ApplyParamTags(OpenApiParameter parameter, ParameterFilterContext context)
        {
            if (!(context.ParameterInfo.Member is MethodInfo methodInfo)) return;

            // If method is from a constructed generic type, look for comments from the generic type method
            var targetMethod = methodInfo.DeclaringType.IsConstructedGenericType
                ? methodInfo.GetUnderlyingGenericTypeMethod()
                : methodInfo;

            if (targetMethod == null) return;

            var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(targetMethod);

            if (!_xmlDocMembers.TryGetValue(methodMemberName, out var propertyNode)) return;

            var paramNode = propertyNode.SelectSingleNode($"param[@name='{context.ParameterInfo.Name}']");

            if (paramNode != null)
            {
                parameter.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);

                var example = paramNode.GetAttribute("example", "");
                if (string.IsNullOrEmpty(example)) return;

                var exampleAsJson = (parameter.Schema?.ResolveType(context.SchemaRepository) == "string")
                    ? $"\"{example}\""
                    : example;

                parameter.Example = OpenApiAnyFactory.CreateFromJson(exampleAsJson);
            }
        }
    }
}
