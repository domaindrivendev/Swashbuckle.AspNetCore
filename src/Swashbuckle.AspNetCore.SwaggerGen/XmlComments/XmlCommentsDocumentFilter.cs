using System.Xml.XPath;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using System;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsDocumentFilter : IDocumentFilter
    {
        private const string SummaryTag = "summary";

        private readonly IReadOnlyDictionary<string, XPathNavigator> _xmlDocMembers;

        public XmlCommentsDocumentFilter(XPathDocument xmlDoc) : this(XmlCommentsDocumentHelper.GetMemberDictionary(xmlDoc))
        {
        }

        public XmlCommentsDocumentFilter(IReadOnlyDictionary<string, XPathNavigator> xmlDocMembers)
        {
            _xmlDocMembers = xmlDocMembers;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Collect (unique) controller names and types in a dictionary
            var controllerNamesAndTypes = context.ApiDescriptions
                .Select(apiDesc => apiDesc.ActionDescriptor as ControllerActionDescriptor)
                .Where(actionDesc => actionDesc != null)
                .GroupBy(actionDesc => actionDesc.ControllerName)
                .Select(group => new KeyValuePair<string, Type>(group.Key, group.First().ControllerTypeInfo.AsType()));

            foreach (var nameAndType in controllerNamesAndTypes)
            {
                var memberName = XmlCommentsNodeNameHelper.GetMemberNameForType(nameAndType.Value);

                if (!_xmlDocMembers.TryGetValue(memberName, out var typeNode)) return;
                
                var summaryNode = typeNode.SelectSingleNode(SummaryTag);
                if (summaryNode != null)
                {
                    if (swaggerDoc.Tags == null)
                        swaggerDoc.Tags = new List<OpenApiTag>();

                    swaggerDoc.Tags.Add(new OpenApiTag
                    {
                        Name = nameAndType.Key,
                        Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml)
                    });
                }
            }
        }
    }
}
