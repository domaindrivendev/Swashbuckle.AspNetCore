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
        private readonly SwaggerGeneratorOptions _options;

        public XmlCommentsDocumentFilter(XPathDocument xmlDoc)
            : this(xmlDoc, null)
        {
        }

        public XmlCommentsDocumentFilter(XPathDocument xmlDoc, SwaggerGeneratorOptions options) : this(XmlCommentsDocumentHelper.GetMemberDictionary(xmlDoc), options)
        {
        }

        internal XmlCommentsDocumentFilter(IReadOnlyDictionary<string, XPathNavigator> xmlDocMembers, SwaggerGeneratorOptions options)
        {
            _xmlDocMembers = xmlDocMembers;
            _options = options;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Collect (unique) controller names and types in a dictionary
            var controllerNamesAndTypes = context.ApiDescriptions
                .Select(apiDesc => new { ApiDesc = apiDesc, ActionDesc = apiDesc.ActionDescriptor as ControllerActionDescriptor })
                .Where(x => x.ActionDesc != null)
                .GroupBy(x => _options?.TagsSelector(x.ApiDesc).FirstOrDefault() ?? x.ActionDesc.ControllerName)
                .Select(group => new KeyValuePair<string, Type>(group.Key, group.First().ActionDesc.ControllerTypeInfo.AsType()));

            foreach (var nameAndType in controllerNamesAndTypes)
            {
                var memberName = XmlCommentsNodeNameHelper.GetMemberNameForType(nameAndType.Value);

                if (!_xmlDocMembers.TryGetValue(memberName, out var typeNode)) continue;

                var summaryNode = typeNode.SelectFirstChild(SummaryTag);
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
