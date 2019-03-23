using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;

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

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo == null) return;

            // If method is from a constructed generic type, look for comments from the generic type method
            var targetMethod = context.MethodInfo.DeclaringType.IsConstructedGenericType
                ? GetGenericTypeMethodOrNullFor(context.MethodInfo)
                : context.MethodInfo;

            if (targetMethod == null) return;

            var typeMemberName = XmlCommentsMemberNameHelper.GetMemberNameForType(targetMethod.DeclaringType);
            var typeNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, typeMemberName));

            // Apply controller-level tags if any
            if (typeNode != null)
            {
                ApplyResponsesXmlToResponses(operation.Responses, typeNode.Select(ResponsesXPath));
            }

            var methodMemberName = XmlCommentsMemberNameHelper.GetMemberNameForMethod(targetMethod);
            var methodNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, methodMemberName));

            // Apply method-level tags
            if (methodNode != null)
            {
                ApplyMethodXmlToOperation(operation, methodNode);
                ApplyParamsXmlToActionParameters(operation.Parameters, operation.RequestBody, context.ApiDescription, methodNode);
                ApplyResponsesXmlToResponses(operation.Responses, methodNode.Select(ResponsesXPath)); // will override controller-level response tags
            }

            // Special handling for parameters that are bound to model properties
            ApplyPropertiesXmlToPropertyParameters(operation.Parameters, context.ApiDescription);
        }

        private MethodInfo GetGenericTypeMethodOrNullFor(MethodInfo constructedTypeMethod)
        {
            var constructedType = constructedTypeMethod.DeclaringType;
            var genericTypeDefinition = constructedType.GetGenericTypeDefinition();

            // Retrieve list of candidate methods that match name and parameter count
            var candidateMethods = genericTypeDefinition.GetMethods()
                .Where(m =>
                {
                    return (m.Name == constructedTypeMethod.Name)
                        && (m.GetParameters().Length == constructedTypeMethod.GetParameters().Length);
                });


            // If inconclusive, just return null
            return (candidateMethods.Count() == 1) ? candidateMethods.First() : null;
        }

        private void ApplyMethodXmlToOperation(OpenApiOperation operation, XPathNavigator methodNode)
        {
            var summaryNode = methodNode.SelectSingleNode(SummaryXPath);
            if (summaryNode != null)
                operation.Summary = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

            var remarksNode = methodNode.SelectSingleNode(RemarksXPath);
            if (remarksNode != null)
                operation.Description = XmlCommentsTextHelper.Humanize(remarksNode.InnerXml);
        }

        private void ApplyParamsXmlToActionParameters(
            IList<OpenApiParameter> parameters,
            OpenApiRequestBody requestBody,
            ApiDescription apiDescription,
            XPathNavigator methodNode)
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

            if (requestBody != null)
            {
                var actionParameter = apiDescription.ParameterDescriptions
                    .FirstOrDefault(p => p.IsFromBody());

                if (actionParameter != null)
                {
                    var paramNode = methodNode.SelectSingleNode(string.Format(ParamXPath, actionParameter.Name));
                    if (paramNode != null)
                        requestBody.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);
                }
            }
        }

        private void ApplyResponsesXmlToResponses(IDictionary<string, OpenApiResponse> responses, XPathNodeIterator responseNodes)
        {
            while (responseNodes.MoveNext())
            {
                var code = responseNodes.Current.GetAttribute("code", "");
                var response = responses.ContainsKey(code)
                    ? responses[code]
                    : responses[code] = new OpenApiResponse();

                response.Description = XmlCommentsTextHelper.Humanize(responseNodes.Current.InnerXml);
            }
        }

        private void ApplyPropertiesXmlToPropertyParameters(
            IList<OpenApiParameter> parameters,
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