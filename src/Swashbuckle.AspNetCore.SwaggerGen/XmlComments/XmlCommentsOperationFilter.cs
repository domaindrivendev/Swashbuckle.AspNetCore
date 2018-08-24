using System;
using System.Linq;
using System.Xml.XPath;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsOperationFilter : IOperationFilter
    {
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryXPath = "summary";
        private const string RemarksXPath = "remarks";
        private const string ParamXPath = "param[@name='{0}']";
        private const string ResponsesXPath = "response";

        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsOperationFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            var controllerActionDescriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor == null) return;

            var memberName = XmlCommentsMemberNameHelper.GetMemberNameForMethod(controllerActionDescriptor.MethodInfo);
            var methodNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, memberName));

            if (methodNode != null)
            {
                ApplyMethodXmlToOperation(operation, methodNode);
                ApplyParamsXmlToActionParameters(operation.Parameters, methodNode, context.ApiDescription);
                ApplyResponsesXmlToResponses(operation.Responses, methodNode.Select(ResponsesXPath));
            }

            // Special handling for parameters that are bound to model properties
            ApplyPropertiesXmlToPropertyParameters(operation.Parameters, context.ApiDescription);
        }

        private void ApplyMethodXmlToOperation(Operation operation, XPathNavigator methodNode)
        {
            var summaryNode = methodNode.SelectSingleNode(SummaryXPath);
            if (summaryNode != null)
                operation.Summary = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

            var remarksNode = methodNode.SelectSingleNode(RemarksXPath);
            if (remarksNode != null)
                operation.Description = XmlCommentsTextHelper.Humanize(remarksNode.InnerXml);
        }

        private void ApplyParamsXmlToActionParameters(
            IList<IParameter> parameters,
            XPathNavigator methodNode,
            ApiDescription apiDescription)
        {
            if (parameters == null) return;

            foreach (var parameter in parameters)
            {
                // Check for a corresponding action parameter?
                var actionParameter = apiDescription.ActionDescriptor.Parameters
                    .FirstOrDefault(p => parameter.Name.Equals(
                        (p.BindingInfo?.BinderModelName ?? p.Name), StringComparison.OrdinalIgnoreCase));
                if (actionParameter == null) continue;

                var paramNode = methodNode.SelectSingleNode(string.Format(ParamXPath, actionParameter.Name));
                if (paramNode != null)
                    parameter.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);
            }
        }

        private void ApplyResponsesXmlToResponses(IDictionary<string, Response> responses, XPathNodeIterator responseNodes)
        {
            while (responseNodes.MoveNext())
            {
                var code = responseNodes.Current.GetAttribute("code", "");
                var response = responses.ContainsKey(code)
                    ? responses[code]
                    : responses[code] = new Response();

                response.Description = XmlCommentsTextHelper.Humanize(responseNodes.Current.InnerXml);
            }
        }

        private void ApplyPropertiesXmlToPropertyParameters(
            IList<IParameter> parameters,
            ApiDescription apiDescription)
        {
            if (parameters == null) return;

            foreach (var parameter in parameters)
            {
                // Check for a corresponding  API parameter (from ApiExplorer) that's property-bound?
                var propertyParam = apiDescription.ParameterDescriptions
                    .Where(p => p.ModelMetadata?.ContainerType != null && p.ModelMetadata?.PropertyName != null)
                    .FirstOrDefault(p => parameter.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                if (propertyParam == null) continue;

                var metadata = propertyParam.ModelMetadata;
                var memberInfo = metadata.ContainerType.GetMember(metadata.PropertyName).FirstOrDefault();
                if (memberInfo == null) continue;

                var memberName = XmlCommentsMemberNameHelper.GetMemberNameForMember(memberInfo);
                var memberNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, memberName));
                if (memberNode == null) continue;

                var summaryNode = memberNode.SelectSingleNode(SummaryXPath);
                if (summaryNode != null)
                    parameter.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            }
        }
    }
}