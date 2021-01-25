using System;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsSchemaFilter : ISchemaFilter
    {
        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsSchemaFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            ApplyTypeTags(schema, context.Type);

            // If it's for a C# field/property and it's NOT bound to a request parameter (e.g. via [FromRoute], [FromQuery] etc.),
            // then the field/property tags can be applied here. If it is bound to a request parameter, the field/property tags
            // will be applied a level up on the corresponding OpenApiParameter (see XmlCommentsParameterFilter).

            if (context.MemberInfo != null && context.ParameterInfo == null)
            {
                ApplyMemberTags(schema, context);
            }
        }

        private void ApplyTypeTags(OpenApiSchema schema, Type type)
        {
            var typeMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(type);
            var typeSummaryNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{typeMemberName}']/summary");

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

            var summaryNode = fieldOrPropertyNode.SelectSingleNode("summary");
            if (summaryNode != null)
                schema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

            var exampleNode = fieldOrPropertyNode.SelectSingleNode("example");
            if (exampleNode != null)
            {
                var exampleAsJson = (schema.ResolveType(context.SchemaRepository) == "string")
                    ? $"\"{exampleNode.InnerXml}\""
                    : exampleNode.InnerXml;

                schema.Example = OpenApiAnyFactory.CreateFromJson(exampleAsJson);
            }
        }
    }
}
