using System.Linq;
using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc.Controllers;
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

            var commentId = XmlCommentsIdHelper.GetCommentIdForMethod(controllerActionDescriptor.MethodInfo);
            var methodNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, commentId));
            if (methodNode == null) return;

            var summaryNode = methodNode.SelectSingleNode(SummaryXPath);
            if (summaryNode != null)
                operation.Summary = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

            var remarksNode = methodNode.SelectSingleNode(RemarksXPath);
            if (remarksNode != null)
                operation.Description = XmlCommentsTextHelper.Humanize(remarksNode.InnerXml);

            ApplyParamComments(operation, methodNode, context);

            ApplyResponseComments(operation, methodNode);
        }

        private static void ApplyParamComments(Operation operation, XPathNavigator methodNode, OperationFilterContext context)
        {
            if (operation.Parameters == null) return;

            foreach (var parameter in operation.Parameters)
            {
                // Inspect context to find the corresponding action parameter
                // NOTE: If a parameter binding is present (e.g. [FromQuery(Name..)]), then the lookup needs
                // to be against the "bound" name and not the actual parameter name
                var actionParameter = context.ApiDescription.ActionDescriptor
                    .Parameters
                    .FirstOrDefault(paramDesc =>
                        (paramDesc.BindingInfo != null && paramDesc.BindingInfo.BinderModelName == parameter.Name)
                        || paramDesc.Name == parameter.Name
                     );
                if (actionParameter == null) continue;

                var paramNode = methodNode.SelectSingleNode(string.Format(ParamXPath, actionParameter.Name));
                if (paramNode != null)
                    parameter.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);
            }
        }

        private static void ApplyResponseComments(Operation operation, XPathNavigator methodNode)
        {
            var responseNodes = methodNode.Select(ResponsesXPath);
            while (responseNodes.MoveNext())
            {
                var responseNode = responseNodes.Current;
                var code = responseNode.GetAttribute("code", "");

                var response = operation.Responses.ContainsKey(code)
                    ? operation.Responses[code]
                    : operation.Responses[code] = new Response();

                response.Description = XmlCommentsTextHelper.Humanize(responseNode.InnerXml);
            }
        }
    }
}