using System;
using System.Globalization;
using System.Reflection;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsOperationFilter : IOperationFilter
    {
        private readonly CultureInfo _сulture;
        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsOperationFilter(XPathDocument xmlDoc, CultureInfo сulture = null)
        {
            _сulture = сulture;
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo == null) return;

            // If method is from a constructed generic type, look for comments from the generic type method
            var targetMethod = context.MethodInfo.DeclaringType.IsConstructedGenericType
                ? context.MethodInfo.GetUnderlyingGenericTypeMethod()
                : context.MethodInfo;

            if (targetMethod == null) return;

            ApplyControllerTags(operation, targetMethod.DeclaringType);
            ApplyMethodTags(operation, targetMethod);
        }

        private void ApplyControllerTags(OpenApiOperation operation, Type controllerType)
        {
            var typeMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(controllerType);
            var responseNodes = _xmlNavigator.Select($"/doc/members/member[@name='{typeMemberName}']/response");
            ApplyResponseTags(operation, responseNodes);
        }

        private void ApplyMethodTags(OpenApiOperation operation, MethodInfo methodInfo)
        {
            var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(methodInfo);
            var methodNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{methodMemberName}']");

            if (methodNode == null) return;

            var summaryNode = methodNode.GetLocalizedNode("summary", _сulture);
            if (summaryNode != null)
                operation.Summary = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

            var remarksNode = methodNode.GetLocalizedNode("remarks", _сulture);
            if (remarksNode != null)
                operation.Description = XmlCommentsTextHelper.Humanize(remarksNode.InnerXml);

            var responseNodes = methodNode.Select("response");
            ApplyResponseTags(operation, responseNodes);
        }

        private void ApplyResponseTags(OpenApiOperation operation, XPathNodeIterator responseNodes)
        {
            if (_сulture != null)
            {
                foreach (XPathNavigator responseNode in responseNodes)
                {
                    var code = responseNode.GetAttribute("code", "");
                    var response = operation.Responses.ContainsKey(code)
                        ? operation.Responses[code]
                        : operation.Responses[code] = new OpenApiResponse();

                    if (!string.IsNullOrWhiteSpace(responseNode.XmlLang)
                        && new CultureInfo(responseNode.XmlLang).TwoLetterISOLanguageName.Equals(_сulture.TwoLetterISOLanguageName))
                    {
                        response.Description = XmlCommentsTextHelper.Humanize(responseNode.InnerXml);
                    }
                    else if (string.IsNullOrWhiteSpace(responseNode.XmlLang) && string.IsNullOrWhiteSpace(response.Description))
                    {
                        response.Description = XmlCommentsTextHelper.Humanize(responseNode.InnerXml);
                    }
                }
            }
            else
            {
                foreach (XPathNavigator responseNode in responseNodes)
                {
                    if (string.IsNullOrWhiteSpace(responseNode?.XmlLang))
                    {
                        var code = responseNode?.GetAttribute("code", "");
                        var response = operation.Responses.ContainsKey(code)
                            ? operation.Responses[code]
                            : operation.Responses[code] = new OpenApiResponse();

                        response.Description = XmlCommentsTextHelper.Humanize(responseNode?.InnerXml);
                    }
                }
            }
        }
    }
}