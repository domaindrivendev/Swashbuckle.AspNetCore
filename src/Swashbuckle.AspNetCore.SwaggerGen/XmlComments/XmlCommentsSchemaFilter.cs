using System.Xml.XPath;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class XmlCommentsSchemaFilter(IReadOnlyDictionary<string, XPathNavigator> xmlDocMembers, SwaggerGeneratorOptions options) : ISchemaFilter
{
    private readonly IReadOnlyDictionary<string, XPathNavigator> _xmlDocMembers = xmlDocMembers;
    private readonly SwaggerGeneratorOptions _options = options;

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        ApplyTypeTags(schema, context.Type);

        if (context.MemberInfo != null)
        {
            ApplyMemberTags(schema, context);
        }
    }

    private void ApplyTypeTags(IOpenApiSchema schema, Type type)
    {
        var typeMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(type);

        if (!_xmlDocMembers.TryGetValue(typeMemberName, out var memberNode)) return;

        var typeSummaryNode = memberNode.SelectFirstChild("summary");

        if (typeSummaryNode != null)
        {
            schema.Description = XmlCommentsTextHelper.Humanize(typeSummaryNode.InnerXml, _options?.XmlCommentEndOfLine);
        }
    }

    private void ApplyMemberTags(IOpenApiSchema schema, SchemaFilterContext context)
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
                {
                    schema.Description = XmlCommentsTextHelper.Humanize(summaryNode, _options?.XmlCommentEndOfLine);
                }

                if (schema is OpenApiSchema concrete)
                {
                    var example = recordDefaultConstructorProperty.GetAttribute("example");
                    if (!string.IsNullOrEmpty(example))
                    {
                        TrySetExample(concrete, context, example);
                    }
                }
            }
        }

        if (_xmlDocMembers.TryGetValue(fieldOrPropertyMemberName, out var fieldOrPropertyNode))
        {
            var summaryNode = fieldOrPropertyNode.SelectFirstChild("summary");
            if (summaryNode != null)
            {
                schema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml, _options?.XmlCommentEndOfLine);
            }

            if (schema is OpenApiSchema concrete)
            {
                var exampleNode = fieldOrPropertyNode.SelectFirstChild("example");
                TrySetExample(concrete, context, exampleNode?.Value);
            }
        }
    }

    private static void TrySetExample(OpenApiSchema schema, SchemaFilterContext context, string example)
    {
        if (example != null)
        {
            schema.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, schema, example);
        }
    }
}
