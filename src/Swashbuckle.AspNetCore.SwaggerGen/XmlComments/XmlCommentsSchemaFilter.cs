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

        internal XmlCommentsSchemaFilter(IReadOnlyDictionary<string, XPathNavigator> xmlDocMembers)
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

            var typeSummaryNode = memberNode.SelectFirstChild("summary");

            if (typeSummaryNode != null)
            {
                schema.Description = XmlCommentsTextHelper.Humanize(typeSummaryNode.InnerXml);
            }
        }

        private void ApplyMemberTags(OpenApiSchema schema, SchemaFilterContext context)
        {
            var fieldOrPropertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(context.MemberInfo);

            var recordTypeName = XmlCommentsNodeNameHelper.GetMemberNameForType(context.MemberInfo.DeclaringType);

            if (_xmlDocMembers.TryGetValue(recordTypeName, out var recordTypeNode))
            {
                XPathNavigator recordDefaultConstructorProperty = recordTypeNode.SelectFirstChildWithAttribute("param", "name", context.MemberInfo.Name);

                if (recordDefaultConstructorProperty != null)
                {
                    var summaryNode = recordDefaultConstructorProperty.Value;
                    if (summaryNode != null)
                        schema.Description = XmlCommentsTextHelper.Humanize(summaryNode);

                    var example = recordDefaultConstructorProperty.GetAttribute("example");
                    if (!string.IsNullOrEmpty(example))
                    {
                        TrySetExample(schema, context, example);
                    }
                }
            }

            if (_xmlDocMembers.TryGetValue(fieldOrPropertyMemberName, out var fieldOrPropertyNode))
            {
                var summaryNode = fieldOrPropertyNode.SelectFirstChild("summary");
                if (summaryNode != null)
                    schema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

                var exampleNode = fieldOrPropertyNode.SelectFirstChild("example");
                TrySetExample(schema, context, exampleNode?.Value);
            }
        }

        private static void TrySetExample(OpenApiSchema schema, SchemaFilterContext context, string example)
        {
            if (example == null)
                return;

            schema.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, schema, example);
        }
    }
}
