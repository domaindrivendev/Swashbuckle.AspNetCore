using System.Xml.XPath;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsPlainDescriptionHumanizer : IDescriptionHumanizer
    {
        public string Humanize(XPathNavigator text)
        {
            return XmlCommentsTextHelper.Humanize(text.InnerXml);
        }
    }
}