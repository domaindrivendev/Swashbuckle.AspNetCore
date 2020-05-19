using System.Xml.XPath;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsMarkdownDescriptionHumanizer : IDescriptionHumanizer
    {
        public string Humanize(XPathNavigator text)
        {
            return XmlTransform.ToMarkdown(text.InnerXml);
        }
    }
}