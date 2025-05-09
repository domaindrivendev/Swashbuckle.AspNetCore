using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class XmlCommentsDocumentFilter(IReadOnlyDictionary<string, XPathNavigator> xmlDocMembers, SwaggerGeneratorOptions options) : IDocumentFilter
{
    private const string SummaryTag = "summary";

    private readonly IReadOnlyDictionary<string, XPathNavigator> _xmlDocMembers = xmlDocMembers;
    private readonly SwaggerGeneratorOptions _options = options;

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

            if (!_xmlDocMembers.TryGetValue(memberName, out var typeNode))
            {
                continue;
            }

            var summaryNode = typeNode.SelectFirstChild(SummaryTag);
            if (summaryNode != null)
            {
                swaggerDoc.Tags ??= [];

                var name = nameAndType.Key;
                var tag = swaggerDoc.Tags.FirstOrDefault((p) => p?.Name == name);

                if (tag is null)
                {
                    tag = new() { Name = name };
                    swaggerDoc.Tags.Add(tag);
                }

                tag.Description ??= XmlCommentsTextHelper.Humanize(summaryNode.InnerXml, _options?.XmlCommentEndOfLine);
            }
        }
    }
}
