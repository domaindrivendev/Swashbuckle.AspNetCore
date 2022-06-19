using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class XmlCommentsDocumentHelper
    {
        public static IReadOnlyDictionary<string, XPathNavigator> GetMemberDictionary(XPathDocument xmlDoc)
        {
            return xmlDoc.CreateNavigator()
                .Select("/doc/members/member")
                .OfType<XPathNavigator>()
                .ToDictionary(memberNode => memberNode.GetAttribute("name", ""));
        }
    }
}
