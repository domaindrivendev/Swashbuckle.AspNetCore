using System.Linq;
using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.SwaggerGen.Generator;

namespace Swashbuckle.SwaggerGen.Annotations
{
    public class XmlCommentsOperationFilter : IOperationFilter
    {
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryTag = "summary";
        private const string RemarksTag = "remarks";
        private const string ParameterTag = "param";

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

            var summaryNode = methodNode.SelectSingleNode(SummaryTag);
            if (summaryNode != null)
                operation.Summary = summaryNode.ExtractContent();

            var remarksNode = methodNode.SelectSingleNode(RemarksTag);
            if (remarksNode != null)
                operation.Description = remarksNode.ExtractContent();

            ApplyParamComments(operation, methodNode);
        }

        private static void ApplyParamComments(Operation operation, XPathNavigator methodNode)
        {
            if (operation.Parameters == null) return;

            var paramNodes = methodNode.Select(ParameterTag);
            while (paramNodes.MoveNext())
            {
                var paramNode = paramNodes.Current;
                var parameter = operation.Parameters
                    .SingleOrDefault(param => param.Name == paramNode.GetAttribute("name", ""));
                if (parameter != null)
                    parameter.Description = paramNode.ExtractContent();
            }
        }
    }
}