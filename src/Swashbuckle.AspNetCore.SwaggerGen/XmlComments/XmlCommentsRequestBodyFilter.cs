using System.Globalization;
using System.Reflection;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsRequestBodyFilter : IRequestBodyFilter
    {
        private readonly CultureInfo _сulture;
        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsRequestBodyFilter(XPathDocument xmlDoc, CultureInfo сulture = null)
        {
            _сulture = сulture;
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
        {
            var bodyParameterDescription = context.BodyParameterDescription;

            if (bodyParameterDescription == null) return;

            var propertyInfo = bodyParameterDescription.PropertyInfo();
            if (propertyInfo != null)
            {
                ApplyPropertyTags(requestBody, context, propertyInfo);
                return;
            }

            var parameterInfo = bodyParameterDescription.ParameterInfo();
            if (parameterInfo != null)
            {
                ApplyParamTags(requestBody, context, parameterInfo);
                return;
            }
        }

        private void ApplyPropertyTags(OpenApiRequestBody requestBody, RequestBodyFilterContext context, PropertyInfo propertyInfo)
        {
            var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(propertyInfo);
            var propertyNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{propertyMemberName}']");

            if (propertyNode == null) return;

            var summaryNode = propertyNode.GetLocalizedNode("summary", _сulture);
            if (summaryNode != null)
                requestBody.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

            var exampleNode = propertyNode.GetLocalizedNode("example", _сulture);
            if (exampleNode == null) return;

            foreach (var mediaType in requestBody.Content.Values)
            {
                var exampleAsJson = (mediaType.Schema?.ResolveType(context.SchemaRepository) == "string")
                    ? $"\"{exampleNode.ToString()}\""
                    : exampleNode.ToString();

                mediaType.Example = OpenApiAnyFactory.CreateFromJson(exampleAsJson);
            }
        }

        private void ApplyParamTags(OpenApiRequestBody requestBody, RequestBodyFilterContext context, ParameterInfo parameterInfo)
        {
            if (!(parameterInfo.Member is MethodInfo methodInfo)) return;

            // If method is from a constructed generic type, look for comments from the generic type method
            var targetMethod = methodInfo.DeclaringType.IsConstructedGenericType
                ? methodInfo.GetUnderlyingGenericTypeMethod()
                : methodInfo;

            if (targetMethod == null) return;

            var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(targetMethod);
            var paramNode = _xmlNavigator.GetLocalizedNode(
                $"/doc/members/member[@name='{methodMemberName}']/param[@name='{parameterInfo.Name}']", _сulture);

            if (paramNode != null)
            {
                requestBody.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);

                var example = paramNode.GetAttribute("example", "");
                if (string.IsNullOrEmpty(example)) return;

                foreach (var mediaType in requestBody.Content.Values)
                {
                    var exampleAsJson = (mediaType.Schema?.ResolveType(context.SchemaRepository) == "string")
                        ? $"\"{example}\""
                        : example;

                    mediaType.Example = OpenApiAnyFactory.CreateFromJson(exampleAsJson);
                }
            }
        }
    }
}
