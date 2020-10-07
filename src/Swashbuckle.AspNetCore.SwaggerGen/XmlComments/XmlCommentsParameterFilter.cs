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
        private const string SummaryTag = "summary";
        private const string RemarksTag = "remarks";
        private const string ExampleTag = "example";

        private readonly XPathNavigator _xmlNavigator;
        private readonly bool _includeRemarksFromXmlComments;

        public XmlCommentsParameterFilter(XPathDocument xmlDoc, bool includeRemarksFromXmlComments = false)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
            _includeRemarksFromXmlComments = includeRemarksFromXmlComments;
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
            var propertyNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{propertyMemberName}']");

            if (propertyNode == null) return;

            var summaryNode = propertyNode.SelectSingleNode(SummaryTag);
            if (summaryNode != null)
            {
                parameter.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

                if (_includeRemarksFromXmlComments)
                {
                    var remarksNode = propertyNode.SelectSingleNode(RemarksTag);
                    if (remarksNode != null && !string.IsNullOrWhiteSpace(remarksNode.InnerXml))
                    {
                        parameter.Description +=
                            $" ({XmlCommentsTextHelper.Humanize(remarksNode.InnerXml)})";
                    }
                }
            }

            var exampleNode = propertyNode.SelectSingleNode(ExampleTag);
            if (exampleNode != null)
            {
                var exampleString = XmlCommentsTextHelper.Humanize(exampleNode.InnerXml);
                parameter.Example = ConvertToOpenApiType(propertyInfo.PropertyType, parameter.Schema, exampleString);
            }
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

                var example = paramNode.GetAttribute("example", "");
                if (!string.IsNullOrEmpty(example))
                {
                    parameter.Example = ConvertToOpenApiType(parameterInfo.ParameterType, parameter.Schema, example);
                }
            }
        }

        private static IOpenApiAny ConvertToOpenApiType(Type type, OpenApiSchema schema, string stringValue)
        {
            object typedValue;

            try
            {
                typedValue = TypeDescriptor.GetConverter(type).ConvertFrom(
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
