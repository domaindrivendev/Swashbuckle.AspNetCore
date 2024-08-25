using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsRequestBodyFilter : IRequestBodyFilter
    {
        private readonly IReadOnlyDictionary<string, XPathNavigator> _xmlDocMembers;

        public XmlCommentsRequestBodyFilter(XPathDocument xmlDoc) : this(XmlCommentsDocumentHelper.GetMemberDictionary(xmlDoc))
        {
        }

        internal XmlCommentsRequestBodyFilter(IReadOnlyDictionary<string, XPathNavigator> xmlDocMembers)
        {
            _xmlDocMembers = xmlDocMembers;
        }

        public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
        {
            var parameterDescription =
                context.BodyParameterDescription ??
                context.FormParameterDescriptions.FirstOrDefault((p) => p is not null);

            if (parameterDescription is null)
            {
                return;
            }

            var propertyInfo = parameterDescription.PropertyInfo();
            if (propertyInfo is not null)
            {
                ApplyPropertyTags(requestBody, context, propertyInfo);
                return;
            }

            var parameterInfo = parameterDescription.ParameterInfo();
            if (parameterInfo is not null)
            {
                ApplyParamTags(requestBody, context, parameterInfo);
            }
        }

        private void ApplyPropertyTags(OpenApiRequestBody requestBody, RequestBodyFilterContext context, PropertyInfo propertyInfo)
        {
            var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(propertyInfo);

            if (!_xmlDocMembers.TryGetValue(propertyMemberName, out var propertyNode)) return;

            var summaryNode = propertyNode.SelectFirstChild("summary");
            if (summaryNode is not null)
            {
                requestBody.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            }

            var exampleNode = propertyNode.SelectFirstChild("example");
            if (exampleNode is null || requestBody.Content?.Count is 0)
            {
                return;
            }

            var example = exampleNode.ToString();

            foreach (var mediaType in requestBody.Content.Values)
            {
                mediaType.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, mediaType.Schema, example);
            }
        }

        private void ApplyParamTags(OpenApiRequestBody requestBody, RequestBodyFilterContext context, ParameterInfo parameterInfo)
        {
            if (parameterInfo.Member is not MethodInfo methodInfo)
            {
                return;
            }

            // If method is from a constructed generic type, look for comments from the generic type method
            var targetMethod = methodInfo.DeclaringType.IsConstructedGenericType
                ? methodInfo.GetUnderlyingGenericTypeMethod()
                : methodInfo;

            if (targetMethod is null)
            {
                return;
            }

            var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(targetMethod);

            if (!_xmlDocMembers.TryGetValue(methodMemberName, out var propertyNode)) return;

            var paramNode = propertyNode.SelectFirstChildWithAttribute("param", "name", parameterInfo.Name);

            if (paramNode is not null)
            {
                requestBody.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);

                var example = paramNode.GetAttribute("example", "");
                if (!string.IsNullOrEmpty(example))
                {
                    foreach (var mediaType in requestBody.Content.Values)
                    {
                        mediaType.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, mediaType.Schema, example);
                    }
                }
            }
        }
    }
}
