using System.Xml.XPath;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class XmlCommentsSchemaFilter(IReadOnlyDictionary<string, XPathNavigator> xmlDocMembers, SwaggerGeneratorOptions options) : ISchemaFilter
{
    private readonly IReadOnlyDictionary<string, XPathNavigator> _xmlDocMembers = xmlDocMembers;
    private readonly SwaggerGeneratorOptions _options = options;

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        ApplyTypeTags(schema, context);

        if (context.MemberInfo != null)
        {
            ApplyMemberTags(schema, context);
        }

        if (context.Type.IsEnum)
        {
            ApplyEnumMemberDescriptions(schema, context);
        }
    }

    private void ApplyTypeTags(IOpenApiSchema schema, SchemaFilterContext context)
    {
        var typeMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(context.Type);

        if (!_xmlDocMembers.TryGetValue(typeMemberName, out var memberNode)) return;

        var typeSummaryNode = memberNode.SelectFirstChild("summary");

        if (typeSummaryNode != null && (context.MemberInfo is null || schema.Description is null))
        {
            // For a member's schema the type's summary is only a fallback: it must not
            // overwrite a description a member's summary has already provided, such as by
            // an XmlCommentsSchemaFilter for another XML comments file when comments are
            // included from multiple assemblies.
            // See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/3240.
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

    /// <summary>
    /// Populates <c>description</c> on <c>oneOf</c> branches of enum schemas generated with
    /// <see cref="SchemaGeneratorOptions.UseOneOfForEnumMemberDescriptions"/>, using the
    /// <c>&lt;summary&gt;</c> XML documentation of each enum member.
    /// See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/3978
    /// </summary>
    private void ApplyEnumMemberDescriptions(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.OneOf == null || schema.OneOf.Count == 0) return;

        // Each oneOf branch is expected to hold a single-value "enum" (see SchemaGenerator).
        // Match branches to enum fields by comparing the serialized value.
        var branchesByJson = new Dictionary<string, IOpenApiSchema>();
        foreach (var branch in schema.OneOf)
        {
            if (branch.Enum?.Count == 1)
            {
                var json = branch.Enum[0]?.ToJsonString();
                if (json != null && !branchesByJson.ContainsKey(json))
                {
                    branchesByJson[json] = branch;
                }
            }
        }

        if (branchesByJson.Count == 0) return;

        var enumType = context.Type;
        foreach (var field in enumType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
        {
            var memberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(field);
            if (!_xmlDocMembers.TryGetValue(memberName, out var memberNode)) continue;

            var summaryNode = memberNode.SelectFirstChild("summary");
            if (summaryNode == null) continue;

            var description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml, _options?.XmlCommentEndOfLine);
            if (string.IsNullOrEmpty(description)) continue;

            // Try to match the field to a oneOf branch: the serialized enum value is either
            // the member name (string enums, e.g. via JsonStringEnumConverter) or the numeric value.
            var fieldValue = field.GetValue(null);
            var candidateJsonValues = new[]
            {
                "\"" + field.Name + "\"",  // string-serialized: "MemberName"
                System.Convert.ToInt64(fieldValue).ToString(System.Globalization.CultureInfo.InvariantCulture) // numeric: 2
            };

            foreach (var candidate in candidateJsonValues)
            {
                if (branchesByJson.TryGetValue(candidate, out var branch) && string.IsNullOrEmpty(branch.Description))
                {
                    branch.Description = description;
                    break;
                }
            }
        }
    }
}
