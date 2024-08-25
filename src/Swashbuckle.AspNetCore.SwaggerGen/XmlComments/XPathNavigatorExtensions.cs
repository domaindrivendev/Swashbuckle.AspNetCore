using System.Linq;
using System.Xml.XPath;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal static class XPathNavigatorExtensions
    {
        internal static XPathNavigator SelectFirstChild(this XPathNavigator navigator, string name)
        {
            return navigator.SelectChildren(name, "")
                    ?.Cast<XPathNavigator>()
                    .FirstOrDefault();
        }

        internal static XPathNavigator SelectFirstChildWithAttribute(this XPathNavigator navigator, string childName, string attributeName, string attributeValue)
        {
            return navigator.SelectChildren(childName, "")
                    ?.Cast<XPathNavigator>()
                    .FirstOrDefault(n => n.GetAttribute(attributeName, "") == attributeValue);
        }
    }
}
