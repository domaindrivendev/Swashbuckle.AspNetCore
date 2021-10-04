using System;
using System.Globalization;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsSchemaFilter : ISchemaFilter
    {
        private readonly CultureInfo _сulture;
        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsSchemaFilter(XPathDocument xmlDoc, CultureInfo сulture = null)
        {
            _сulture = сulture;
            _xmlNavigator = xmlDoc.CreateNavigator();
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
            var typeSummaryNode = _xmlNavigator.GetLocalizedNode($"/doc/members/member[@name='{typeMemberName}']/summary", _сulture);

            if (typeSummaryNode != null)
            {
                schema.Description = XmlCommentsTextHelper.Humanize(typeSummaryNode.InnerXml);
            }
        }

        private void ApplyMemberTags(OpenApiSchema schema, SchemaFilterContext context)
        {
            var fieldOrPropertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(context.MemberInfo);
            var fieldOrPropertyNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{fieldOrPropertyMemberName}']");

            if (fieldOrPropertyNode == null) return;

            var summaryNode = fieldOrPropertyNode.GetLocalizedNode("summary", _сulture);
            if (summaryNode != null)
                schema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

            var exampleNode = fieldOrPropertyNode.GetLocalizedNode("example", _сulture);
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
