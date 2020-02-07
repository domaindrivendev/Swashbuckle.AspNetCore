using System.Reflection;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsRequestBodyFilter : IRequestBodyFilter
    {
        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsRequestBodyFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
        {
            var bodyParameterDescription = context.BodyParameterDescription;

            if (bodyParameterDescription == null) return;

            var propertyInfo = bodyParameterDescription.PropertyInfo();
            if (propertyInfo != null)
            {
                ApplyPropertyTags(requestBody, propertyInfo);
                return;
            }

            var parameterInfo = bodyParameterDescription.ParameterInfo();
            if (parameterInfo != null)
            {
                ApplyParamTags(requestBody, parameterInfo);
                return;
            }
        }

        private void ApplyPropertyTags(OpenApiRequestBody requestBody, PropertyInfo propertyInfo)
        {
            var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(propertyInfo);
            var propertySummaryNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{propertyMemberName}']/summary");

            if (propertySummaryNode != null)
                requestBody.Description = XmlCommentsTextHelper.Humanize(propertySummaryNode.InnerXml);
        }

        private void ApplyParamTags(OpenApiRequestBody requestBody, ParameterInfo parameterInfo)
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
                requestBody.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);
            }
        }
    }
}
