using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsSchemaFilter : ISchemaFilter
    {
        private readonly Dictionary<string, XPathNavigator> _docMembers;

        public XmlCommentsSchemaFilter(XPathDocument xmlDoc)
        {
            _docMembers = xmlDoc.CreateNavigator()
                .Select("/doc/members/member")
                .OfType<XPathNavigator>()
                .ToDictionary(memberNode => memberNode.GetAttribute("name", ""));
        }

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            ApplyTypeTags(schema, context.Type);

            if (context.MemberInfo != null)
            {
                ApplyMemberTags(schema, context);
            }
        }

        private void ApplyTypeTags(OpenApiSchema schema, Type type)
        {
            var typeMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(type);

            if (!_docMembers.TryGetValue(typeMemberName, out var memberNode)) return;

            var typeSummaryNode = memberNode.SelectSingleNode("summary");

            if (typeSummaryNode != null)
            {
                schema.Description = XmlCommentsTextHelper.Humanize(typeSummaryNode.InnerXml);
            }
        }

        private void ApplyMemberTags(OpenApiSchema schema, SchemaFilterContext context)
        {
            var fieldOrPropertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(context.MemberInfo);

            if (!_docMembers.TryGetValue(fieldOrPropertyMemberName, out var fieldOrPropertyNode)) return;

            var summaryNode = fieldOrPropertyNode.SelectSingleNode("summary");
            if (summaryNode != null)
                schema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

            var exampleNode = fieldOrPropertyNode.SelectSingleNode("example");
            if (exampleNode != null)
            {
                var exampleAsJson = (schema.ResolveType(context.SchemaRepository) == "string") && !exampleNode.Value.Equals("null")
                    ? $"\"{exampleNode.ToString()}\""
                    : exampleNode.ToString();

                schema.Example = OpenApiAnyFactory.CreateFromJson(exampleAsJson);
            }
        }
    }
}
