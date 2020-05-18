﻿using System.Reflection;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsParameterFilter : IParameterFilter
    {
        private XPathNavigator _xmlNavigator;
        private readonly IDescriptionHumanizer _descriptionHumanizer;

        public XmlCommentsParameterFilter(XPathDocument xmlDoc, IDescriptionHumanizer descriptionHumanizer)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
            _descriptionHumanizer = descriptionHumanizer;
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
                parameter.Description = _descriptionHumanizer.Humanize(propertySummaryNode);
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
                parameter.Description = _descriptionHumanizer.Humanize(paramNode);
            }
        }
    }
}
