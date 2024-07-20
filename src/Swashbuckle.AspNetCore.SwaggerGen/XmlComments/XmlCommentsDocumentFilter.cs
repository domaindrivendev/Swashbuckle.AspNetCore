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
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryTag = "summary";

        private readonly XPathNavigator _xmlNavigator;
        private readonly SwaggerGeneratorOptions _options;

        public XmlCommentsDocumentFilter(XPathDocument xmlDoc)
            : this(xmlDoc, null)
        {
        }

        public XmlCommentsDocumentFilter(XPathDocument xmlDoc, SwaggerGeneratorOptions options)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
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
                var typeNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, memberName));

                if (typeNode != null)
                {
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
}
