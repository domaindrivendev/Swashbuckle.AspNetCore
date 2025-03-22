using System.Xml.XPath;

namespace Swashbuckle.AspNetCore.SwaggerGen;

internal static class XmlCommentsDocumentHelper
{
    internal static Dictionary<string, XPathNavigator> CreateMemberDictionary(XPathDocument xmlDoc)
    {
        var members = xmlDoc.CreateNavigator()
            .SelectFirstChild("doc")
            ?.SelectFirstChild("members")
            ?.SelectChildren("member")
            ?.OfType<XPathNavigator>();

        return members?.ToDictionary(memberNode => memberNode.GetAttribute("name")) ?? [];
    }
}
