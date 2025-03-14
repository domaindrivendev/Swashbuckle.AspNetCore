using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.XPath;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsParameterFilter : IParameterFilter
    {
        private readonly IReadOnlyDictionary<string, XPathNavigator> _xmlDocMembers;
        private readonly SwaggerGeneratorOptions _options;

        public XmlCommentsParameterFilter(XPathDocument xmlDoc) : this(XmlCommentsDocumentHelper.CreateMemberDictionary(xmlDoc), null)
        {
        }

        [ActivatorUtilitiesConstructor]
        internal XmlCommentsParameterFilter(IReadOnlyDictionary<string, XPathNavigator> xmlDocMembers, SwaggerGeneratorOptions options)
        {
            _xmlDocMembers = xmlDocMembers;
            _options = options;
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

            var summaryNode = propertyNode.SelectFirstChild("summary");
            if (summaryNode != null)
            {
                parameter.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml, _options?.XmlCommentEndOfLine);
                parameter.Schema.Description = null; // no need to duplicate
            }

            var exampleNode = propertyNode.SelectFirstChild("example");
            if (exampleNode == null) return;

            parameter.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, parameter.Schema, exampleNode.ToString());
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

            XPathNavigator paramNode = propertyNode.SelectFirstChildWithAttribute("param", "name", context.ParameterInfo.Name);

            if (paramNode != null)
            {
                parameter.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml, _options?.XmlCommentEndOfLine);

                var example = paramNode.GetAttribute("example");
                if (string.IsNullOrEmpty(example)) return;

                parameter.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, parameter.Schema, example);
            }
        }
    }
}
