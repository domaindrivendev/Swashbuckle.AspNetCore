using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Xml.XPath;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsParameterFilter : IParameterFilter
    {
        private XPathNavigator _xmlNavigator;

        public XmlCommentsParameterFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            if (context.PropertyInfo != null)
            {
                ApplyPropertyTags(parameter, context.PropertyInfo);
            }
            else if (context.ParameterInfo != null)
            {
                ApplyParamTags(parameter, context.ParameterInfo);
            }
        }

        private void ApplyPropertyTags(OpenApiParameter parameter, PropertyInfo propertyInfo)
        {
            var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(propertyInfo);
            var propertySummaryNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{propertyMemberName}']/summary");

            if (propertySummaryNode != null)
                parameter.Description = XmlCommentsTextHelper.Humanize(propertySummaryNode.InnerXml);
        }

        private void ApplyParamTags(OpenApiParameter parameter, ParameterInfo parameterInfo)
        {
            if (!(parameterInfo.Member is MethodInfo methodInfo)) return;

            // If method is from a constructed generic type, look for comments from the generic type method
            var targetMethod = methodInfo.DeclaringType.IsConstructedGenericType
                ? methodInfo.GetUnderlyingGenericTypeMethod()
                : methodInfo;

            if (targetMethod == null) return;

            var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(targetMethod);
            var paramNode = _xmlNavigator.SelectSingleNode(
                $"/doc/members/member[@name='{methodMemberName}']/param[@name='{parameterInfo.Name}']");

            if (paramNode != null)
            {
                parameter.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);

                var example = paramNode.GetAttribute("example", string.Empty);
                if (!string.IsNullOrEmpty(example))
                {
                    parameter.Example = ConvertToOpenApiType(parameterInfo.ParameterType, parameter.Schema, example);
                }
            }
        }

        private static IOpenApiAny ConvertToOpenApiType(Type memberType, OpenApiSchema schema, string stringValue)
        {
            object typedValue;

            try
            {
                typedValue = TypeDescriptor.GetConverter(memberType).ConvertFrom(
                    context: null,
                    culture: CultureInfo.InvariantCulture,
                    stringValue);
            }
            catch (Exception)
            {
                return null;
            }

            return OpenApiAnyFactory.CreateFor(schema, typedValue);
        }
    }
}
