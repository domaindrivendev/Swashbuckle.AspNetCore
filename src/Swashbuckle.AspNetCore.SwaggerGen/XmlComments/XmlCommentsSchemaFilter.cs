using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Xml.XPath;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsSchemaFilter : ISchemaFilter
    {
        private readonly IReadOnlyDictionary<string, XPathNavigator> _xmlDocMembers;

        public XmlCommentsSchemaFilter(XPathDocument xmlDoc) : this(XmlCommentsDocumentHelper.GetMemberDictionary(xmlDoc))
        {
        }

        public XmlCommentsSchemaFilter(IReadOnlyDictionary<string, XPathNavigator> xmlDocMembers)
        {
            _xmlDocMembers = xmlDocMembers;
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

            if (!_xmlDocMembers.TryGetValue(typeMemberName, out var memberNode)) return;

            var typeSummaryNode = memberNode.SelectSingleNode("summary");

            if (typeSummaryNode != null)
            {
                schema.Description = XmlCommentsTextHelper.Humanize(typeSummaryNode.InnerXml);
            }
        }

        private void ApplyMemberTags(OpenApiSchema schema, SchemaFilterContext context)
        {
            var fieldOrPropertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(context.MemberInfo);

            if (!_xmlDocMembers.TryGetValue(fieldOrPropertyMemberName, out var fieldOrPropertyNode)) return;

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
