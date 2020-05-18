using System.Xml.XPath;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface IDescriptionHumanizer
    {
        string Humanize(XPathNavigator text);
    }
}