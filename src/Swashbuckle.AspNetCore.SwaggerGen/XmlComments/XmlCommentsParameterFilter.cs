using System.Reflection;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsParameterFilter : IParameterFilter
    {
        private const string SummaryTag = "summary";
        private const string ExampleTag = "example";
        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsParameterFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            if (context.PropertyInfo != null)
                ApplyPropertyTags(parameter, context);
            else if (context.ParameterInfo != null) ApplyParamTags(parameter, context);
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
            var paramNode =
                _xmlNavigator.SelectSingleNodeRecursive(methodMemberName,
                    $"param[@name='{context.ParameterInfo.Name}']");

            if (paramNode == null) return;
            parameter.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);

            var example = paramNode.GetAttribute(ExampleTag, "");
            if (string.IsNullOrEmpty(example)) return;

            var exampleAsJson = parameter.Schema?.ResolveType(context.SchemaRepository) == "string"
                ? $"\"{example}\""
                : example;

            parameter.Example = OpenApiAnyFactory.CreateFromJson(exampleAsJson);
        }

        private void ApplyPropertyTags(OpenApiParameter parameter, ParameterFilterContext context)
        {
            var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(context.PropertyInfo);
            var propertyNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{propertyMemberName}']");

            if (propertyNode == null) return;

            var summaryNode = _xmlNavigator.SelectSingleNodeRecursive(propertyMemberName, SummaryTag);
            if (summaryNode != null)
            {
                parameter.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
                parameter.Schema.Description = null; // no need to duplicate
            }

            var exampleNode = _xmlNavigator.SelectSingleNodeRecursive(propertyMemberName, ExampleTag);
            if (exampleNode == null) return;

            var exampleAsJson = parameter.Schema?.ResolveType(context.SchemaRepository) == "string"
                ? $"\"{exampleNode}\""
                : exampleNode.ToString();

            parameter.Example = OpenApiAnyFactory.CreateFromJson(exampleAsJson);
        }
    }
}