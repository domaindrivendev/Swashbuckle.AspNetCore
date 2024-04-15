using System.Xml.XPath;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsDocumentFilter : IDocumentFilter
    {
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryTag = "summary";

        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsDocumentFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Collect (unique) controller names, types and custom tags (defined by the first TagsAttribute value) in a dictionary
            var controllers = context.ApiDescriptions
                .Select(apiDesc => apiDesc.ActionDescriptor as ControllerActionDescriptor)
                .Where(actionDesc => actionDesc != null)
                .GroupBy(actionDesc => actionDesc.ControllerName)
                .Select(group => new KeyValuePair<string, ControllerInfo>(group.Key, GetControllerInfo(group)));

            swaggerDoc.Tags ??= new List<OpenApiTag>();
            foreach (var (controllerName, controllerInfo) in controllers)
            {
                var memberName = XmlCommentsNodeNameHelper.GetMemberNameForType(controllerInfo.ControllerType);
                var typeNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, memberName));

                var description = GetXmlDescriptionOrNull(typeNode);

                swaggerDoc.Tags.Add(new OpenApiTag
                {
                    Name = controllerInfo.CustomTagName ?? controllerName,
                    Description = description
                });
            }
            swaggerDoc.Tags = swaggerDoc.Tags.OrderBy(x => x.Name).ToList();
        }

        private class ControllerInfo
        {
            public Type ControllerType { get; set; }
            public string CustomTagName { get; set; }
        }

        private static ControllerInfo GetControllerInfo(IGrouping<string, ControllerActionDescriptor> group)
        {
            var controllerInfo = new ControllerInfo
            {
                ControllerType = group.First().ControllerTypeInfo.AsType()
            };

#if NET6_0_OR_GREATER
            controllerInfo.CustomTagName =
                group.First().MethodInfo.DeclaringType?.GetCustomAttribute<TagsAttribute>()?.Tags[0];
#endif

            return controllerInfo;
        }

        private static string GetXmlDescriptionOrNull(XPathNavigator typeNode)
        {
            if (typeNode != null)
            {
                var summaryNode = typeNode.SelectSingleNode(SummaryTag);
                if (summaryNode != null)
                {
                    return XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
                }
            }

            return null;
        }
    }
}
