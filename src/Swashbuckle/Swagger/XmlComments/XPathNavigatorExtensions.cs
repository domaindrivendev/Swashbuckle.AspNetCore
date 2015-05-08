using System.Xml.XPath;
using System.Text.RegularExpressions;

namespace Swashbuckle.Swagger.XmlComments
{
    public static class XPathNavigatorExtensiosn
    {
        private static Regex ParamPattern = new Regex(@"<(see|paramref) (name|cref)=""([TPF]{1}:)?(?<display>.+?)"" />");
        private static Regex ConstPattern = new Regex(@"<c>(?<display>.+?)</c>");

        public static string ExtractContent(this XPathNavigator node)
        {
            if (node == null) return null;

            return ConstPattern.Replace(
                ParamPattern.Replace(node.InnerXml, GetParamRefName),
                GetConstRefName).Trim();
        }

        private static string GetConstRefName(Match match)
        {
            if (match.Groups.Count != 2) return null;

            return match.Groups["display"].Value;
        }

        private static string GetParamRefName(Match match)
        {
            if (match.Groups.Count != 5) return null;

            return "{" + match.Groups["display"].Value + "}";
        }
    }
}
