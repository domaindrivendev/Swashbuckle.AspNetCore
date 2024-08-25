using System.Collections.Generic;
using System.Linq;
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

        if (members == null)
        {
            return new Dictionary<string, XPathNavigator>();
        }

        return members.ToDictionary(memberNode => memberNode.GetAttribute("name"));
    }
}
